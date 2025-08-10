using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects.Mobs;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer;
using AlbionOnlineSniffer.Core.Models.ResponseObj;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Handlers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core
{
    public class LocationEventsTests
    {
        private static readonly byte[] SampleXorCode = new byte[] { 0x10, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };

        private static ILoggerFactory CreateLoggerFactory()
        {
            return LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        }

        private static void LoadMinimalOffsets(params (string name, byte[] values)[] entries)
        {
            // Monta um offsets.json temporário com apenas os campos necessários
            var dict = new Dictionary<string, int[]>();
            foreach (var (name, values) in entries)
            {
                dict[name] = values.Select(b => (int)b).ToArray();
            }

            var json = JsonSerializer.Serialize(dict);
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, json);

            var loader = new PacketOffsetsLoader(CreateLoggerFactory().CreateLogger<PacketOffsetsLoader>());
            loader.LoadOffsets(tmp);
        }

        private static byte[] MakeEncryptedPositionBytes(float x, float y, byte[] xor)
        {
            var xBytes = BitConverter.GetBytes(x);
            var yBytes = BitConverter.GetBytes(y);

            ApplyXor(xBytes, xor, 0);
            ApplyXor(yBytes, xor, 4);

            return xBytes.Concat(yBytes).ToArray();
        }

        private static void ApplyXor(byte[] bytes4, byte[] saltBytes8, int saltPos)
        {
            for (var i = 0; i < bytes4.Length; i++)
            {
                var saltIndex = i % (saltBytes8.Length - saltPos) + saltPos;
                bytes4[i] ^= saltBytes8[saltIndex];
            }
        }

        private sealed class TestMoveEventHandler : MoveEventHandler
        {
            public TestMoveEventHandler(PlayersHandler p, MobsHandler m, EventDispatcher d) : base(p, m, d) { }
            public Task InvokeAsync(MoveEvent e) => base.OnActionAsync(e);
        }

        private sealed class TestKeySyncEventHandler : KeySyncEventHandler
        {
            public TestKeySyncEventHandler(PlayersHandler p, EventDispatcher d) : base(p, d) { }
            public Task InvokeAsync(KeySyncEvent e) => base.OnActionAsync(e);
        }

        private sealed class TestNewCharacterEventHandler : NewCharacterEventHandler
        {
            public TestNewCharacterEventHandler(PlayersHandler ph, LocalPlayerHandler lh, ConfigHandler ch, EventDispatcher d)
                : base(ph, lh, ch, d) { }
            public Task InvokeAsync(NewCharacterEvent e) => base.OnActionAsync(e);
        }

        [Fact]
        public async Task MoveEvent_ShouldFillDecryptedPositions_AfterKeySync()
        {
            // Arrange: offsets para KeySync e Move
            LoadMinimalOffsets(("KeySync", new byte[] { 0 }), ("Move", new byte[] { 0, 1 }));

            var loggerFactory = CreateLoggerFactory();
            var dispatcher = new EventDispatcher(loggerFactory.CreateLogger<EventDispatcher>());
            var playersHandler = new PlayersHandler(new List<PlayerItems>());
            var mobsHandler = new MobsHandler(new List<MobInfo>());

            var keySyncHandler = new TestKeySyncEventHandler(playersHandler, dispatcher);
            var moveHandler = new TestMoveEventHandler(playersHandler, mobsHandler, dispatcher);

            // Simula o recebimento do KeySync
            var ksParams = new Dictionary<byte, object> { { 0, SampleXorCode } };
            var keySyncEvent = new KeySyncEvent(ksParams);
            await keySyncHandler.InvokeAsync(keySyncEvent);

            // Cria MoveEvent com bytes encriptados
            var id = 12345;
            var originalPos = (x: 100.5f, y: 200.25f);
            var originalNewPos = (x: 101.5f, y: 201.25f);
            var speed = 5.5f;
            var flags = (byte)(1 | 2); // Speed + NewPosition

            var positionBytes = MakeEncryptedPositionBytes(originalPos.x, originalPos.y, SampleXorCode);
            var newPositionBytes = MakeEncryptedPositionBytes(originalNewPos.x, originalNewPos.y, SampleXorCode);

            // Monta o blob esperado pelo MoveEvent: flags em [0], pos em [9..16], speed em [18..21], newpos em [22..29]
            var blob = new byte[30];
            blob[0] = flags;
            Array.Copy(positionBytes, 0, blob, 9, 8);
            Array.Copy(BitConverter.GetBytes(speed), 0, blob, 18, 4);
            Array.Copy(newPositionBytes, 0, blob, 22, 8);

            var moveParams = new Dictionary<byte, object>
            {
                { 0, id },
                { 1, blob }
            };
            var moveEvent = new MoveEvent(moveParams);

            // Act
            await moveHandler.InvokeAsync(moveEvent);

            // Assert: evento enriquecido com posições decriptadas (note a inversão X/Y na construção do Vector2)
            Assert.NotNull(moveEvent.Position);
            Assert.NotNull(moveEvent.NewPosition);

            // Vector2 é (Y, X) conforme PlayersHandler
            Assert.InRange(moveEvent.Position!.Value.X, originalPos.y - 0.0001f, originalPos.y + 0.0001f);
            Assert.InRange(moveEvent.Position!.Value.Y, originalPos.x - 0.0001f, originalPos.x + 0.0001f);

            Assert.InRange(moveEvent.NewPosition!.Value.X, originalNewPos.y - 0.0001f, originalNewPos.y + 0.0001f);
            Assert.InRange(moveEvent.NewPosition!.Value.Y, originalNewPos.x - 0.0001f, originalNewPos.x + 0.0001f);

            // E no JSON publicado (formato do Program.cs), Position aparece dentro de Data
            var envelope = new
            {
                EventType = nameof(MoveEvent),
                Timestamp = DateTime.UtcNow,
                Data = moveEvent
            };
            var json = JsonSerializer.Serialize(envelope);
            Assert.Contains("\"Position\":", json);
            Assert.Contains("\"NewPosition\":", json);
        }

        [Fact]
        public async Task NewCharacterEvent_ShouldDecryptPosition_AndSetPlayerPosition_AfterKeySync()
        {
            // Arrange offsets mínimos para NewCharacter e KeySync
            LoadMinimalOffsets(
                ("KeySync", new byte[] { 0 }),
                ("NewCharacter", new byte[] { 0, 1, 8, 51, 53, 16, 20, 22, 23, 40, 43 })
            );

            var loggerFactory = CreateLoggerFactory();
            var dispatcher = new EventDispatcher(loggerFactory.CreateLogger<EventDispatcher>());
            var playersHandler = new PlayersHandler(new List<PlayerItems>());
            var localPlayerHandler = new AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer.LocalPlayerHandler(new Dictionary<string, AlbionOnlineSniffer.Core.Models.GameObjects.Localplayer.Cluster>());
            var configHandler = new ConfigHandler();

            var keySyncHandler = new TestKeySyncEventHandler(playersHandler, dispatcher);
            var newCharHandler = new TestNewCharacterEventHandler(playersHandler, localPlayerHandler, configHandler, dispatcher);

            // KeySync
            var ksParams = new Dictionary<byte, object> { { 0, SampleXorCode } };
            var keySyncEvent = new KeySyncEvent(ksParams);
            await keySyncHandler.InvokeAsync(keySyncEvent);

            // Dados originais
            var id = 9876;
            var name = "Tester";
            var guild = "G";
            var alliance = "A";
            var faction = (byte)AlbionOnlineSniffer.Core.Utility.Faction.NoPVP;
            var encPos = MakeEncryptedPositionBytes(10.25f, 20.75f, SampleXorCode);
            var speed = 5.5f;
            var hpCur = 100; var hpMax = 200;
            var equipments = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var spells = new int[] { 0, 0, 0, 0 };

            var p = new Dictionary<byte, object>
            {
                { 0, id },
                { 1, name },
                { 8, guild },
                { 51, alliance },
                { 53, faction },
                { 16, encPos },
                { 20, speed },
                { 22, hpCur },
                { 23, hpMax },
                { 40, equipments },
                { 43, spells },
            };

            var ev = new NewCharacterEvent(p);

            // Act
            await newCharHandler.InvokeAsync(ev);

            // Assert: PlayersHandler contém o player com posição decriptada
            Assert.True(playersHandler.playersList.ContainsKey(id));
            var player = playersHandler.playersList[id];
            Assert.NotEqual(Vector2.Zero, player.Position);
        }

        [Fact]
        public void NewMobEvent_ShouldParsePlainPosition()
        {
            LoadMinimalOffsets(("NewMobEvent", new byte[] { 0, 1, 8, 13, 14, 33 }));

            var pos = new float[] { 123.0f, 456.0f };
            var p = new Dictionary<byte, object>
            {
                { 0, 11 },
                { 1, 22 },
                { 8, pos },
                { 13, 0 },
                { 14, 0 },
                { 33, 0 },
            };

            var ev = new NewMobEvent(p);
            Assert.True(Math.Abs(ev.Position.X - pos[1]) < 0.001f);
            Assert.True(Math.Abs(ev.Position.Y - pos[0]) < 0.001f);
        }

        [Fact]
        public void NewLootChestEvent_ShouldParsePlainPosition()
        {
            LoadMinimalOffsets(("NewLootChest", new byte[] { 0, 1, 3, 17 }));
            var pos = new float[] { 12.5f, 34.75f };
            var p = new Dictionary<byte, object>
            {
                { 0, 1 },
                { 1, pos },
                { 3, "ChestName" },
                { 17, 0 },
            };
            var ev = new NewLootChestEvent(p);
            Assert.True(Math.Abs(ev.Position.X - pos[1]) < 0.001f);
            Assert.True(Math.Abs(ev.Position.Y - pos[0]) < 0.001f);
        }

        [Fact]
        public void NewHarvestableEvent_ShouldParsePlainPosition()
        {
            LoadMinimalOffsets(("NewHarvestableObject", new byte[] { 0, 5, 7, 8, 10, 11 }));
            var pos = new float[] { 1.25f, 2.5f };
            var p = new Dictionary<byte, object>
            {
                { 0, 100 },
                { 5, 1 },
                { 7, 2 },
                { 8, pos },
                { 10, 3 },
                { 11, 4 },
            };
            var ev = new NewHarvestableEvent(p);
            Assert.True(Math.Abs(ev.Position.X - pos[1]) < 0.001f);
            Assert.True(Math.Abs(ev.Position.Y - pos[0]) < 0.001f);
        }
    }
}


