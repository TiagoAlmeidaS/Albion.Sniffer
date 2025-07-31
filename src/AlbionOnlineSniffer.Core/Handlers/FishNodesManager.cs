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
        private readonly NewFishingZoneObjectEventHandler _newFishingZoneObjectHandler;

        public ConcurrentDictionary<int, FishNode> FishNodesList { get; } = new();

        public FishNodesManager(ILogger<FishNodesManager> logger, EventDispatcher eventDispatcher, NewFishingZoneObjectEventHandler newFishingZoneObjectHandler)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            _newFishingZoneObjectHandler = newFishingZoneObjectHandler;
        }

        public void AddFishZone(int id, Vector2 position, int size, int respawnCount)
        {
            lock (FishNodesList)
            {
                if (FishNodesList.ContainsKey(id))
                    FishNodesList.TryRemove(id, out _);
                
                var fishNode = new FishNode(id, position, size, respawnCount);
                FishNodesList.TryAdd(id, fishNode);
                
                _logger.LogInformation("Fish Node detectado: ID {Id} em ({X}, {Y})", id, position.X, position.Y);
                
                // Disparar evento de fish node detectado
                _ = _eventDispatcher.DispatchEvent(new FishNodeDetectedEvent(id, position, size, respawnCount));
            }
        }

        /// <summary>
        /// Processa um evento NewFishingZoneObject
        /// </summary>
        public async Task<FishNode?> ProcessNewFishingZone(Dictionary<byte, object> parameters)
        {
            try
            {
                var fishNode = await _newFishingZoneObjectHandler.HandleNewFishingZoneObject(parameters);
                if (fishNode != null)
                {
                    AddFishZone(fishNode.Id, fishNode.Position, fishNode.Size, fishNode.RespawnCount);
                    
                    _logger.LogInformation("Nova zona de pesca processada: ID {Id} (Size: {Size}, Respawn: {Respawn})", 
                        fishNode.Id, fishNode.Size, fishNode.RespawnCount);
                    return fishNode;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar NewFishingZone: {Message}", ex.Message);
                return null;
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