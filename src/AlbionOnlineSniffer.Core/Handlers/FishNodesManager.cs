// Classe responsável por gerenciar o estado e a coleção de fish nodes do jogo.
// Não processa eventos diretamente, apenas mantém e manipula o estado dos fish nodes.
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class FishNodesManager : IFishNodesManager
    {
        private readonly ILogger<FishNodesManager> _logger;
        private readonly EventDispatcher _eventDispatcher;

        public ConcurrentDictionary<int, FishNode> FishNodesList { get; } = new();

        public FishNodesManager(ILogger<FishNodesManager> logger, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
        }

        public void AddFishZone(int id, Vector2 position, int size, int respawnCount)
        {
            lock (FishNodesList)
            {
                if (FishNodesList.ContainsKey(id))
                    FishNodesList.TryRemove(id, out _);
                
                FishNodesList.TryAdd(id, new FishNode(id, position, size, respawnCount));
                
                _logger.LogInformation("Fish Node detectado: ID {Id} em ({X}, {Y})", id, position.X, position.Y);
                
                // Disparar evento de fish node detectado
                _ = _eventDispatcher.DispatchEvent(new FishNodeDetectedEvent(id, position, size, respawnCount));
            }
        }

        public void Remove(int id)
        {
            lock (FishNodesList)
            {
                if (FishNodesList.TryRemove(id, out FishNode? fishNode))
                {
                    _logger.LogInformation("Fish Node removido: ID {Id}", id);
                }
            }
        }

        public void Clear()
        {
            lock (FishNodesList)
            {
                FishNodesList.Clear();
                _logger.LogInformation("Todos os Fish Nodes foram removidos");
            }
        }
    }
} 