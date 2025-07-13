// Classe responsável por gerenciar o estado e a coleção de mobs do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado dos mobs.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class MobsManager : IMobsManager
    {
        public ConcurrentDictionary<int, Mob> MobsList { get; } = new();
        private readonly List<MobInfo> _mobInfos;
        public MobsManager(List<MobInfo> mobInfos)
        {
            _mobInfos = mobInfos;
        }
        public void AddMob(int id, int typeId, Vector2 position, Health health, byte enchLvl)
        {
            lock (MobsList)
            {
                if (MobsList.ContainsKey(id))
                    MobsList.TryRemove(id, out _);
                MobsList.TryAdd(id, new Mob(id, typeId, position, enchLvl, _mobInfos.Find(x => x.Id == typeId), health));
            }
        }
        public void UpdateMobPosition(int id, byte[] positionBytes, byte[] newPositionBytes, float speed, DateTime time)
        {
            var position = new Vector2(BitConverter.ToSingle(positionBytes, 4), BitConverter.ToSingle(positionBytes, 0));
            var newPosition = new Vector2(BitConverter.ToSingle(newPositionBytes, 4), BitConverter.ToSingle(newPositionBytes, 0));
            lock (MobsList)
            {
                if (MobsList.TryGetValue(id, out var mob))
                {
                    mob.Position = position;
                    mob.Speed = speed;
                    mob.Time = time;
                    mob.NewPosition = newPosition;
                }
            }
        }
        public void SyncMobsPositions()
        {
            lock (MobsList)
            {
                foreach (var p in MobsList.Values.ToList())
                {
                    if (p == null || p.Speed == 0) continue;
                    Vector2 posDiff = p.Position - p.NewPosition;
                    if (posDiff == Vector2.Zero) continue;
                    p.Position -= posDiff * (float)((DateTime.UtcNow - p.Time).TotalSeconds / (posDiff.Length() / (p.Speed / 10)));
                }
            }
        }
        public void Remove(int id)
        {
            lock (MobsList)
                MobsList.TryRemove(id, out _);
        }
        public void Clear()
        {
            lock (MobsList)
                MobsList.Clear();
        }
        public void UpdateMobCharge(int mobId, int charge)
        {
            lock (MobsList)
            {
                if (MobsList.TryGetValue(mobId, out var mob))
                {
                    mob.Charge = charge;
                }
            }
        }
        public void UpdateHealth(int id, int health)
        {
            lock (MobsList)
            {
                if (MobsList.TryGetValue(id, out var mob))
                {
                    mob.Health.Value = health;
                }
            }
        }
    }
} 