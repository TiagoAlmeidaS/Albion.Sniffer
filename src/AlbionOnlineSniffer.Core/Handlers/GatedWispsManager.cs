// Classe responsável por gerenciar o estado e a coleção de gated wisps do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado dos gated wisps.
using System;
using System.Collections.Concurrent;
using System.Numerics;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class GatedWispsManager : IGatedWispsManager
    {
        public ConcurrentDictionary<int, GatedWisp> GatedWispsList { get; } = new();
        public void AddWispInGate(int id, Vector2 position)
        {
            lock (GatedWispsList)
            {
                if (GatedWispsList.ContainsKey(id))
                    GatedWispsList.TryRemove(id, out _);
                GatedWispsList.TryAdd(id, new GatedWisp(id, position));
            }
        }
        public void Remove(int id)
        {
            lock (GatedWispsList)
                GatedWispsList.TryRemove(id, out _);
        }
        public void Clear()
        {
            lock (GatedWispsList)
                GatedWispsList.Clear();
        }
    }
} 