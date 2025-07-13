using System.Numerics;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Interfaces
{
    public interface IDungeonsManager
    {
        void AddDungeon(int id, string type, Vector2 position, int charges);
        void Remove(int id);
        void Clear();
    }
} 