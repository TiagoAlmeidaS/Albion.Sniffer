using System.Numerics;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    public interface ILootChestsManager
    {
        void AddWorldChest(int id, Vector2 position, string name, int enchLvl);
        void Remove(int id);
        void Clear();
        int GetCharge(string name, int enchLvl);
    }
} 