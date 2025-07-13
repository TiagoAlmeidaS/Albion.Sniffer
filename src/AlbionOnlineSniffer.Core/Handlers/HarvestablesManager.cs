// Classe responsável por gerenciar o estado e a coleção de harvestables do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado dos harvestables.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class HarvestablesManager : IHarvestablesManager
    {
        public ConcurrentDictionary<int, Harvestable> HarvestableList { get; } = new();
        private readonly Dictionary<int, string> _harvestableTypes;
        private readonly LocalPlayerHandler _localPlayerHandler;
        public HarvestablesManager(Dictionary<int, string> harvestableTypes, LocalPlayerHandler localPlayerHandler)
        {
            _harvestableTypes = harvestableTypes;
            _localPlayerHandler = localPlayerHandler;
        }
        public void AddHarvestable(int id, int type, int tier, Vector2 position, int count, int charge)
        {
            lock (HarvestableList)
            {
                if (HarvestableList.ContainsKey(id))
                    HarvestableList.TryRemove(id, out _);
                HarvestableList.TryAdd(id, new Harvestable(id, LoadHarvestableType(type), tier, position, count, charge));
            }
        }
        public void RemoveHarvestables()
        {
            lock (HarvestableList)
            {
                var toRemove = HarvestableList.Where(t => System.Numerics.Vector2.Distance(t.Value.Position, _localPlayerHandler.LocalPlayer.Position) > 70).Select(t => t.Key).ToList();
                foreach (var key in toRemove)
                {
                    HarvestableList.TryRemove(key, out _);
                }
            }
        }
        public void UpdateHarvestable(int id, int count, int charge)
        {
            lock (HarvestableList)
            {
                if (HarvestableList.TryGetValue(id, out var temp))
                {
                    temp.Count = count;
                    temp.Charge = charge;
                }
            }
        }
        public void Clear()
        {
            lock (HarvestableList)
                HarvestableList.Clear();
        }
        private string LoadHarvestableType(int type)
        {
            if (_harvestableTypes != null && _harvestableTypes.ContainsKey(type))
                return _harvestableTypes[type];
            return "null";
        }
    }
} 