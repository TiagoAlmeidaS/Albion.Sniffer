using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Gerenciador de loot chests baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class LootChestsManager
    {
        private readonly ILogger<LootChestsManager> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly ConcurrentDictionary<int, LootChest> _lootChests = new();
        private readonly NewLootChestEventHandler _newLootChestHandler;
        private readonly EventDispatcher _eventDispatcher;

        public LootChestsManager(ILogger<LootChestsManager> logger, PositionDecryptor positionDecryptor, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _eventDispatcher = eventDispatcher;
            
            // Criar logger específico para o handler
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var newLootChestLogger = loggerFactory.CreateLogger<NewLootChestEventHandler>();
            
            _newLootChestHandler = new NewLootChestEventHandler(newLootChestLogger, positionDecryptor);
        }

        /// <summary>
        /// Adiciona um novo loot chest
        /// </summary>
        public void AddLootChest(int id, Vector2 position, string name, int charge)
        {
            lock (_lootChests)
            {
                if (_lootChests.ContainsKey(id))
                {
                    _lootChests.TryRemove(id, out LootChest? existingChest);
                }

                var lootChest = new LootChest(id, position, name, charge);
                _lootChests.TryAdd(id, lootChest);
                
                _logger.LogInformation("Loot chest adicionado: {Name} (ID: {Id})", name, id);

                // Disparar evento de loot chest detectado
                _ = _eventDispatcher.DispatchEvent(new LootChestDetectedEvent(lootChest));
            }
        }

        /// <summary>
        /// Remove um loot chest
        /// </summary>
        public void RemoveLootChest(int id)
        {
            lock (_lootChests)
            {
                if (_lootChests.TryRemove(id, out LootChest? chest))
                {
                    _logger.LogInformation("Loot chest removido: {Name} (ID: {Id})", chest?.Name ?? "Unknown", id);
                }
            }
        }

        /// <summary>
        /// Atualiza um loot chest
        /// </summary>
        public void UpdateLootChest(int id, int charge)
        {
            lock (_lootChests)
            {
                if (_lootChests.TryGetValue(id, out LootChest? chest))
                {
                    chest.Charge = charge;
                }
            }
        }

        /// <summary>
        /// Processa um evento NewLootChest
        /// </summary>
        public async Task ProcessNewLootChest(Dictionary<byte, object> parameters)
        {
            var lootChest = await _newLootChestHandler.HandleNewLootChest(parameters);
            if (lootChest is not null)
            {
                AddLootChest(lootChest.Id, lootChest.Position, lootChest.Name, lootChest.Charge);
            }
        }

        /// <summary>
        /// Obtém todos os loot chests
        /// </summary>
        public IEnumerable<LootChest> GetAllLootChests()
        {
            return _lootChests.Values;
        }

        /// <summary>
        /// Obtém um loot chest específico
        /// </summary>
        public LootChest? GetLootChest(int id)
        {
            _lootChests.TryGetValue(id, out LootChest? chest);
            return chest;
        }

        /// <summary>
        /// Limpa todos os loot chests
        /// </summary>
        public void Clear()
        {
            lock (_lootChests)
            {
                _lootChests.Clear();
                _logger.LogInformation("Todos os loot chests foram removidos");
            }
        }

        /// <summary>
        /// Obtém a contagem de loot chests
        /// </summary>
        public int LootChestCount => _lootChests.Count;
    }
} 