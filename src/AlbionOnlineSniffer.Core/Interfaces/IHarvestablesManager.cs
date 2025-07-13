using System.Numerics;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    public interface IHarvestablesManager
    {
        void AddHarvestable(int id, int type, int tier, Vector2 position, int count, int charge);
        void RemoveHarvestables();
        void UpdateHarvestable(int id, int count, int charge);
        void Clear();
    }
} 