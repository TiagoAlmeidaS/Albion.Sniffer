using System.Numerics;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    public interface IGatedWispsManager
    {
        void AddWispInGate(int id, Vector2 position);
        void Remove(int id);
        void Clear();
    }
} 