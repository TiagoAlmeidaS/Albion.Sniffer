using System;
using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Models.GameObjects.Players;
using AlbionOnlineSniffer.Core.Utility;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    public interface IPlayersManager
    {
        byte[]? XorCode { get; set; }
        float[] Decrypt(byte[] coordinates, int offset = 0);
        void AddPlayer(int id, string name, string guild, string alliance, Vector2 position, Health health, Faction faction, int[] equipments, int[] spells);
        void Remove(int id);
        void Clear();
        void Mounted(int id, bool isMounted);
        void UpdateHealth(int id, int health);
        void SetFaction(int id, Faction faction);
        void RegenerateHealth();
        void UpdateItems(int id, int[] equipment, int[] spells);
        void SetRegeneration(int id, Health health);
        void SyncPlayersPosition();
        void UpdatePlayerPosition(int id, byte[] positionBytes, byte[] newPositionBytes, float speed, DateTime time);
    }
} 