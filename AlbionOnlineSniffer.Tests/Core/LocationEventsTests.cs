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
using AlbionOnlineSniffer.Core;
using Microsoft.Extensions.DependencyInjection;
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

        private static PacketOffsets CreateMinimalOffsets(params (string name, byte[] values)[] entries)
        {
            // Creates a PacketOffsets object with the specified fields
            var offsets = new PacketOffsets();
            
            foreach (var (name, values) in entries)
            {
                var property = typeof(PacketOffsets).GetProperty(name);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(offsets, values);
                }
            }
            
            return offsets;
        }

        private static void SetupPacketOffsetsProvider(PacketOffsets offsets)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => CreateLoggerFactory());
            DependencyProvider.RegisterDataLoader(services, offsets);
            var serviceProvider = services.BuildServiceProvider();
            DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);
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
            public TestNewCharacterEventHandler(PlayersHandler p, EventDispatcher d) : base(p, new LocalPlayerHandler(new Dictionary<string, Cluster>()), new ConfigHandler(), d) { }
            public Task InvokeAsync(NewCharacterEvent e) => base.OnActionAsync(e);
        }

        [Fact]
        public async Task MoveEvent_WithEncryptedPosition_ShouldBeDecryptedByHandler()
        {
            // Arrange - Setup PacketOffsets for KeySync and Move events
            var offsets = CreateMinimalOffsets(
                ("KeySync", new byte[] { 0 }),
                ("Move", new byte[] { 0, 1 })
            );
            SetupPacketOffsetsProvider(offsets);

            var dispatcher = new EventDispatcher(CreateLoggerFactory().CreateLogger<EventDispatcher>());
            var playersHandler = new PlayersHandler(new List<ItemInfo>());
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
        }

        [Fact]
        public async Task NewCharacterEvent_ShouldCreateAndHandleCorrectly()
        {
            // Arrange - Setup PacketOffsets for NewCharacter event
            var offsets = CreateMinimalOffsets(
                ("KeySync", new byte[] { 0 }),
                ("NewCharacter", new byte[] { 0, 1, 2, 3, 4, 5 })
            );
            SetupPacketOffsetsProvider(offsets);

            var dispatcher = new EventDispatcher(CreateLoggerFactory().CreateLogger<EventDispatcher>());
            var playersHandler = new PlayersHandler(new List<ItemInfo>());
            var keySyncHandler = new TestKeySyncEventHandler(playersHandler, dispatcher);
            var newCharHandler = new TestNewCharacterEventHandler(playersHandler, dispatcher);

            // Setup XOR key first
            var ksParams = new Dictionary<byte, object> { { 0, SampleXorCode } };
            var keySyncEvent = new KeySyncEvent(ksParams);
            await keySyncHandler.InvokeAsync(keySyncEvent);

            // Create NewCharacterEvent
            var characterId = 98765;
            var characterName = "TestPlayer";
            var guildName = "TestGuild";
            var allianceName = "TestAlliance";
            var posBytes = MakeEncryptedPositionBytes(150.5f, 250.75f, SampleXorCode);
            var items = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f };

            var p = new Dictionary<byte, object>
            {
                { 0, characterId },
                { 1, characterName },
                { 2, guildName },
                { 3, allianceName },
                { 4, posBytes },
                { 5, items }
            };

            var ev = new NewCharacterEvent(p);

            // Act
            await newCharHandler.InvokeAsync(ev);

            // Assert
            Assert.Equal(characterId, ev.Id);
            Assert.Equal(characterName, ev.Name);
            Assert.Equal(guildName, ev.GuildName);
            Assert.Equal(allianceName, ev.AllianceName);
            Assert.NotNull(ev.PositionBytes);
            Assert.Equal(items, ev.Items);
        }

        [Fact]
        public void NewMobEvent_ShouldParseCorrectly()
        {
            // Arrange - Setup PacketOffsets for NewMobEvent
            var offsets = CreateMinimalOffsets(
                ("NewMobEvent", new byte[] { 0, 1, 2, 3, 4, 5 })
            );
            SetupPacketOffsetsProvider(offsets);

            var mobId = 55555;
            var typeId = 12345;
            var posBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var health = 75.5f;
            var maxHealth = 100.0f;
            var enchLevel = (byte)3;

            var p = new Dictionary<byte, object>
            {
                { 0, mobId },
                { 1, typeId },
                { 2, posBytes },
                { 3, health },
                { 4, maxHealth },
                { 5, enchLevel }
            };

            // Act
            var ev = new NewMobEvent(p);

            // Assert
            Assert.Equal(mobId, ev.Id);
            Assert.Equal(typeId, ev.TypeId);
            Assert.Equal(posBytes, ev.PositionBytes);
            Assert.Equal(health, ev.Health);
            Assert.Equal(maxHealth, ev.MaxHealth);
            Assert.Equal(enchLevel, ev.EnchantmentLevel);
        }

        [Fact]
        public void NewHarvestableEvent_ShouldParseCorrectly()
        {
            // Arrange - Setup PacketOffsets for NewHarvestableObject
            var offsets = CreateMinimalOffsets(
                ("NewHarvestableObject", new byte[] { 0, 1, 2, 3, 4 })
            );
            SetupPacketOffsetsProvider(offsets);

            var harvestId = 77777;
            var harvestTypeId = 98765;
            var posBytes = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
            var tier = (byte)6;
            var charges = (byte)15;

            var p = new Dictionary<byte, object>
            {
                { 0, harvestId },
                { 1, harvestTypeId },
                { 2, posBytes },
                { 3, tier },
                { 4, charges }
            };

            // Act
            var ev = new NewHarvestableEvent(p);

            // Assert
            Assert.Equal(harvestId, ev.Id);
            Assert.Equal(harvestTypeId, ev.TypeId);
            Assert.Equal(posBytes, ev.PositionBytes);
            Assert.Equal(tier, ev.Tier);
            Assert.Equal(charges, ev.Charges);
        }
    }
}


