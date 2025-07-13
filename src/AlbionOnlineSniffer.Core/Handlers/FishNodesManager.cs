// Classe responsável por gerenciar o estado e a coleção de fish nodes do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado dos fish nodes.
using System;
using System.Collections.Concurrent;
using System.Numerics;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class FishNodesManager : IFishNodesManager
    {
        public ConcurrentDictionary<int, FishNode> FishNodesList { get; } = new();
        public void AddFishZone(int id, Vector2 position, int size, int respawnCount)
        {
            lock (FishNodesList)
            {
                if (FishNodesList.ContainsKey(id))
                    FishNodesList.TryRemove(id, out _);
                FishNodesList.TryAdd(id, new FishNode(id, position, size, respawnCount));
            }
        }
        public void Remove(int id)
        {
            lock (FishNodesList)
                FishNodesList.TryRemove(id, out _);
        }
        public void Clear()
        {
            lock (FishNodesList)
                FishNodesList.Clear();
        }
    }
} 