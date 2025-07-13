// Classe responsável por gerenciar o estado e a coleção de jogadores do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado dos players.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class PlayersManager : IPlayersManager
    {
        public ConcurrentDictionary<int, Player> PlayersList { get; } = new();
        private readonly List<PlayerItems> _itemsList;
        public byte[]? XorCode { get; set; }

        public PlayersManager(List<PlayerItems> itemsList)
        {
            _itemsList = itemsList;
        }

        public float[] Decrypt(byte[] coordinates, int offset = 0)
        {
            var code = XorCode;
            if (code == null)
            {
                return new[] { BitConverter.ToSingle(coordinates, offset), BitConverter.ToSingle(coordinates, offset + 4) };
            }
            var x = coordinates.Skip(offset).Take(4).ToArray();
            var y = coordinates.Skip(offset + 4).Take(4).ToArray();
            Decrypt(x, code, 0);
            Decrypt(y, code, 4);
            return new[] { BitConverter.ToSingle(x, 0), BitConverter.ToSingle(y, 0) };
        }

        private static void Decrypt(byte[] bytes4, byte[] saltBytes8, int saltPos)
        {
            for (var i = 0; i < bytes4.Length; i++)
            {
                var saltIndex = i % (saltBytes8.Length - saltPos) + saltPos;
                bytes4[i] ^= saltBytes8[saltIndex];
            }
        }

        public void AddPlayer(int id, string name, string guild, string alliance, Vector2 position, Health health, Faction faction, int[] equipments, int[] spells)
        {
            lock (PlayersList)
            {
                if (PlayersList.ContainsKey(id))
                    PlayersList.TryRemove(id, out _);
                PlayersList.TryAdd(id, new Player(id, name, guild, alliance, position, health, faction, LoadEquipment(equipments), spells));
            }
        }

        public void Remove(int id)
        {
            lock (PlayersList)
                PlayersList.TryRemove(id, out _);
        }

        public void Clear()
        {
            lock (PlayersList)
                PlayersList.Clear();
        }

        public void Mounted(int id, bool isMounted)
        {
            lock (PlayersList)
            {
                if (PlayersList.TryGetValue(id, out var player))
                    player.IsMounted = isMounted;
            }
        }

        public void UpdateHealth(int id, int health)
        {
            lock (PlayersList)
            {
                if (PlayersList.TryGetValue(id, out var player))
                    player.Health.Value = health;
            }
        }

        public void SetFaction(int id, Faction faction)
        {
            lock (PlayersList)
            {
                if (PlayersList.TryGetValue(id, out var player))
                    player.Faction = faction;
            }
        }

        public void RegenerateHealth()
        {
            lock (PlayersList)
            {
                foreach (var p in PlayersList.Values.ToList())
                {
                    if (p != null && p.Health.IsRegeneration)
                        p.Health.Value += (int)p.Health.Regeneration;
                }
            }
        }

        public void UpdateItems(int id, int[] equipment, int[] spells)
        {
            lock (PlayersList)
            {
                if (PlayersList.TryGetValue(id, out var player))
                {
                    player.Equipment = LoadEquipment(equipment);
                    player.Spells = spells;
                }
            }
        }

        public void SetRegeneration(int id, Health health)
        {
            lock (PlayersList)
            {
                if (PlayersList.TryGetValue(id, out var player))
                    player.Health = health;
            }
        }

        public void SyncPlayersPosition()
        {
            lock (PlayersList)
            {
                foreach (var p in PlayersList.Values.ToList())
                {
                    if (p == null || p.IsStanding || p.Speed == 0) continue;
                    Vector2 posDiff = p.Position - p.NewPosition;
                    if (posDiff == Vector2.Zero) continue;
                    p.Position -= posDiff * (float)((DateTime.UtcNow - p.Time).TotalSeconds / (posDiff.Length() / (p.Speed / 10)));
                }
            }
        }

        public void UpdatePlayerPosition(int id, byte[] positionBytes, byte[] newPositionBytes, float speed, DateTime time)
        {
            lock (PlayersList)
            {
                Vector2 position = Vector2.Zero;
                Vector2 newPosition = Vector2.Zero;
                if (XorCode != null)
                {
                    var pos = Decrypt(positionBytes);
                    position = new Vector2(pos[1], pos[0]);
                    var newPos = Decrypt(newPositionBytes);
                    newPosition = new Vector2(newPos[1], newPos[0]);
                }
                if (PlayersList.TryGetValue(id, out var player))
                {
                    player.IsStanding = (player.Position - position).Length() <= 0.05;
                    player.Position = position;
                    player.Speed = speed;
                    player.Time = time;
                    player.NewPosition = newPosition;
                }
            }
        }

        private Equipment LoadEquipment(int[] values)
        {
            Array.Resize(ref values, 8);
            Equipment equipment = new Equipment();
            for (int i = 0; i < values.Length; i++)
            {
                if (_itemsList.Exists(x => x.Id == values[i]))
                {
                    equipment.Items.Add(_itemsList.Find(x => x.Id == values[i]));
                }
                else if (values[i] == 0 || values[i] == -1)
                {
                    equipment.Items.Add(new PlayerItems() { Id = 0, Itempower = 0, Name = "NULL" });
                }
                else
                {
                    equipment.Items.Add(new PlayerItems() { Id = 0, Itempower = 0, Name = "T1_TRASH" });
                }
            }
            equipment.AllItemPower = GetItemPower(equipment.Items);
            return equipment;
        }

        private int GetItemPower(List<PlayerItems> items)
        {
            return items.Sum(i => i.Itempower);
        }
    }
} 