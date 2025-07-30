// Classe responsável por gerenciar o estado e a coleção de dungeons do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado das dungeons.
using System;
using System.Collections.Concurrent;
using System.Numerics;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class DungeonsManager : IDungeonsManager
    {
        public ConcurrentDictionary<int, Dungeon> DungeonsList { get; } = new();
        public void AddDungeon(int id, string type, Vector2 position, int charges)
        {
            lock (DungeonsList)
            {
                if (DungeonsList.ContainsKey(id))
                    DungeonsList.TryRemove(id, out _);
                DungeonsList.TryAdd(id, new Dungeon(id, type, position, charges));
            }
        }
        public void Remove(int id)
        {
            lock (DungeonsList)
                DungeonsList.TryRemove(id, out _);
        }
        public void Clear()
        {
            lock (DungeonsList)
                DungeonsList.Clear();
        }
    }
} 