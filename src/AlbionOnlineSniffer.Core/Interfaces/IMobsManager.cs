using System.Numerics;
using AlbionOnlineSniffer.Core.Models.GameObjects;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    public interface IMobsManager
    {
        void AddMob(int id, int typeId, Vector2 position, Health health, byte enchLvl);
        void UpdateMobPosition(int id, byte[] positionBytes, byte[] newPositionBytes, float speed, System.DateTime time);
        void SyncMobsPositions();
        void Remove(int id);
        void Clear();
        void UpdateMobCharge(int mobId, int charge);
        void UpdateHealth(int id, int health);
    }
} 