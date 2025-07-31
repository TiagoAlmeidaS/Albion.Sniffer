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
    /// Gerenciador de harvestables baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class HarvestablesManager
    {
        private readonly ILogger<HarvestablesManager> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly ConcurrentDictionary<int, Harvestable> _harvestables = new();
        private readonly NewHarvestableEventHandler _newHarvestableHandler;
        private readonly EventDispatcher _eventDispatcher;
        private readonly Dictionary<int, string> _harvestableTypes = new();

        public HarvestablesManager(ILogger<HarvestablesManager> logger, PositionDecryptor positionDecryptor, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _eventDispatcher = eventDispatcher;
            
            // Criar handler usando o ServiceFactory
            _newHarvestableHandler = DependencyProvider.CreateNewHarvestableEventHandler();
            
            // Inicializar tipos de harvestables básicos
            InitializeHarvestableTypes();
        }

        /// <summary>
        /// Adiciona um novo harvestable
        /// </summary>
        public void AddHarvestable(int id, int type, int tier, Vector2 position, int count, int charge)
        {
            lock (_harvestables)
            {
                if (_harvestables.ContainsKey(id))
                {
                    _harvestables.TryRemove(id, out Harvestable? existingHarvestable);
                }

                var typeString = LoadHarvestableType(type);
                var harvestable = new Harvestable(id, typeString, tier, position, count, charge);
                _harvestables.TryAdd(id, harvestable);
                
                _logger.LogInformation("Harvestable adicionado: {Type} T{Level} (ID: {Id})", typeString, tier, id);

                // Disparar evento de harvestable detectado
                _ = _eventDispatcher.DispatchEvent(new HarvestableDetectedEvent(harvestable));
            }
        }

        /// <summary>
        /// Remove harvestables muito distantes do jogador
        /// </summary>
        public void RemoveHarvestables(Vector2 playerPosition, float maxDistance = 70f)
        {
            lock (_harvestables)
            {
                var toRemove = new List<int>();
                
                foreach (var kvp in _harvestables)
                {
                    var distance = Vector2.Distance(kvp.Value.Position, playerPosition);
                    if (distance > maxDistance)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (var id in toRemove)
                {
                    if (_harvestables.TryRemove(id, out Harvestable? harvestable))
                    {
                        _logger.LogDebug("Harvestable removido por distância: {Type} (ID: {Id})", harvestable.Type, id);
                    }
                }
            }
        }

        /// <summary>
        /// Atualiza um harvestable
        /// </summary>
        public void UpdateHarvestable(int id, int count, int charge)
        {
            lock (_harvestables)
            {
                if (_harvestables.TryGetValue(id, out Harvestable? harvestable))
                {
                    harvestable.Count = count;
                    harvestable.Charge = charge;
                    
                    // Disparar evento de harvestable atualizado com posição
                    _ = _eventDispatcher.DispatchEvent(new HarvestableUpdatedEvent(id, count, charge, harvestable.Position));
                }
            }
        }

        /// <summary>
        /// Processa um evento NewHarvestable
        /// </summary>
        public async Task<Harvestable?> ProcessNewHarvestable(Dictionary<byte, object> parameters)
        {
            try
            {
                var harvestable = await _newHarvestableHandler.HandleNewHarvestable(parameters);
                if (harvestable != null)
                {
                    AddHarvestable(harvestable.Id, GetTypeId(harvestable.Type), harvestable.Tier, 
                                  harvestable.Position, harvestable.Count, harvestable.Charge);
                    
                    _logger.LogInformation("Novo harvestable processado: {Type} T{Level} (ID: {Id})", 
                        harvestable.Type, harvestable.Tier, harvestable.Id);
                    return harvestable; // ← RETORNAR O HARVESTABLE CRIADO
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar NewHarvestable: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Obtém todos os harvestables
        /// </summary>
        public IEnumerable<Harvestable> GetAllHarvestables()
        {
            return _harvestables.Values;
        }

        /// <summary>
        /// Obtém um harvestable específico
        /// </summary>
        public Harvestable? GetHarvestable(int id)
        {
            _harvestables.TryGetValue(id, out Harvestable? harvestable);
            return harvestable;
        }

        /// <summary>
        /// Limpa todos os harvestables
        /// </summary>
        public void Clear()
        {
            lock (_harvestables)
            {
                _harvestables.Clear();
                _logger.LogInformation("Todos os harvestables foram removidos");
            }
        }

        /// <summary>
        /// Obtém a contagem de harvestables
        /// </summary>
        public int HarvestableCount => _harvestables.Count;

        /// <summary>
        /// Inicializa os tipos de harvestables
        /// </summary>
        private void InitializeHarvestableTypes()
        {
            _harvestableTypes[1] = "FIBER";
            _harvestableTypes[2] = "HIDE";
            _harvestableTypes[3] = "ORE";
            _harvestableTypes[4] = "WOOD";
            _harvestableTypes[5] = "STONE";
        }

        /// <summary>
        /// Carrega o tipo de harvestable
        /// </summary>
        private string LoadHarvestableType(int type)
        {
            lock (_harvestables)
            {
                if (_harvestableTypes.ContainsKey(type))
                    return _harvestableTypes[type];

                return "UNKNOWN";
            }
        }

        /// <summary>
        /// Obtém o ID do tipo baseado na string
        /// </summary>
        private int GetTypeId(string typeString)
        {
            return typeString switch
            {
                "FIBER" => 1,
                "HIDE" => 2,
                "ORE" => 3,
                "WOOD" => 4,
                "STONE" => 5,
                _ => 0
            };
        }
    }
} 