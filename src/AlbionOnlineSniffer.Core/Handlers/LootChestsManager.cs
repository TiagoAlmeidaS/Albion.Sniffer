// Classe responsável por gerenciar o estado e a coleção de loot chests do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado dos loot chests.
using System;
using System.Collections.Concurrent;
using System.Numerics;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class LootChestsManager : ILootChestsManager
    {
        public ConcurrentDictionary<int, LootChest> LootChestsList { get; } = new();
        public void AddWorldChest(int id, Vector2 position, string name, int enchLvl)
        {
            lock (LootChestsList)
            {
                if (LootChestsList.ContainsKey(id))
                    LootChestsList.TryRemove(id, out _);
                LootChestsList.TryAdd(id, new LootChest(id, position, name, GetCharge(name, enchLvl)));
            }
        }
        public void Remove(int id)
        {
            lock (LootChestsList)
                LootChestsList.TryRemove(id, out _);
        }
        public void Clear()
        {
            lock (LootChestsList)
                LootChestsList.Clear();
        }
        public int GetCharge(string name, int enchLvl)
        {
            if (enchLvl > 0) return enchLvl;
            string[] temp = name.Split('_');
            if (temp.Length < 2) return 0;
            switch (temp[temp.Length - 2])
            {
                case "STANDARD": return 1;
                case "UNCOMMON": return 2;
                case "RARE": return 3;
                case "LEGENDARY": return 4;
            }
            return 0;
        }
    }
} 