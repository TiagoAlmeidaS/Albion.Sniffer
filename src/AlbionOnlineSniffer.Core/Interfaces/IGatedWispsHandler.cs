using System.Numerics;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    public interface IGatedWispsManager
    {
        void AddWispInGate(int id, System.Numerics.Vector2 position);
        void Remove(int id);
        void Clear();
    }
} 