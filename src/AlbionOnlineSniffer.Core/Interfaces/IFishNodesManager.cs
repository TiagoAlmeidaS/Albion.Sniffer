using System.Numerics;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    public interface IFishNodesManager
    {
        void AddFishZone(int id, Vector2 position, int size, int respawnCount);
        void Remove(int id);
        void Clear();
    }
} 