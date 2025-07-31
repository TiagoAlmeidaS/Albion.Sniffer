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
    /// Gerenciador de mobs baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class MobsManager
    {
        private readonly ILogger<MobsManager> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly ConcurrentDictionary<int, Mob> _mobs = new();
        private readonly NewMobEventHandler _newMobHandler;
        private readonly EventDispatcher _eventDispatcher;

        public MobsManager(ILogger<MobsManager> logger, PositionDecryptor positionDecryptor, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _eventDispatcher = eventDispatcher;
            
            // Criar handler usando o ServiceFactory
            _newMobHandler = DependencyProvider.CreateNewMobEventHandler();
        }

        /// <summary>
        /// Adiciona um novo mob
        /// </summary>
        public void AddMob(int id, int typeId, Vector2 position, Health health, byte charge)
        {
            lock (_mobs)
            {
                if (_mobs.ContainsKey(id))
                {
                    _mobs.TryRemove(id, out Mob? existingMob);
                }

                var mobInfo = new MobInfo
                {
                    Id = typeId,
                    Tier = 1,
                    Type = "UNKNOWN",
                    MobName = $"Mob_{typeId}"
                };

                var mob = new Mob(id, typeId, position, charge, mobInfo, health);
                _mobs.TryAdd(id, mob);
                
                _logger.LogInformation("Mob adicionado: {MobName} (ID: {Id})", mobInfo.MobName, id);

                // Disparar evento de mob detectado
                _ = _eventDispatcher.DispatchEvent(new MobDetectedEvent(mob));
            }
        }

        /// <summary>
        /// Remove um mob
        /// </summary>
        public void RemoveMob(int id)
        {
            lock (_mobs)
            {
                if (_mobs.TryRemove(id, out Mob? mob))
                {
                    _logger.LogInformation("Mob removido: {MobName} (ID: {Id})", mob?.MobInfo.MobName ?? "Unknown", id);
                    
                    // Disparar evento de mob removido com posição
                    _ = _eventDispatcher.DispatchEvent(new MobRemovedEvent(id, mob?.Position));
                }
            }
        }

        /// <summary>
        /// Atualiza a posição de um mob
        /// </summary>
        public void UpdateMobPosition(int id, Vector2 position)
        {
            lock (_mobs)
            {
                if (_mobs.TryGetValue(id, out Mob? mob))
                {
                    mob.Position = position;
                    mob.Time = DateTime.UtcNow;
                    
                    // Disparar evento de mob movido
                    _ = _eventDispatcher.DispatchEvent(new GenericGameEvent("MobMoved"));
                }
            }
        }

        /// <summary>
        /// Atualiza a saúde de um mob
        /// </summary>
        public void UpdateMobHealth(int id, int health)
        {
            lock (_mobs)
            {
                if (_mobs.TryGetValue(id, out Mob? mob))
                {
                    mob.Health.Value = health;
                }
            }
        }

        /// <summary>
        /// Atualiza a carga de um mob
        /// </summary>
        public void UpdateMobCharge(int id, int charge)
        {
            lock (_mobs)
            {
                if (_mobs.TryGetValue(id, out Mob? mob))
                {
                    mob.Charge = charge;
                }
            }
        }

        /// <summary>
        /// Processa um evento NewMob
        /// </summary>
        public async Task<Mob?> ProcessNewMob(Dictionary<byte, object> parameters)
        {
            try
            {
                var mob = await _newMobHandler.HandleNewMob(parameters);
                if (mob != null)
                {
                    AddMob(mob.Id, mob.TypeId, mob.Position, mob.Health, (byte)mob.Charge);
                    
                    _logger.LogInformation("Novo mob processado: {MobName} (ID: {Id})", mob.MobInfo.MobName, mob.Id);
                    return mob; // ← RETORNAR O MOB CRIADO
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar NewMob: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Obtém todos os mobs
        /// </summary>
        public IEnumerable<Mob> GetAllMobs()
        {
            return _mobs.Values;
        }

        /// <summary>
        /// Obtém um mob específico
        /// </summary>
        public Mob? GetMob(int id)
        {
            _mobs.TryGetValue(id, out Mob? mob);
            return mob;
        }

        /// <summary>
        /// Limpa todos os mobs
        /// </summary>
        public void Clear()
        {
            lock (_mobs)
            {
                _mobs.Clear();
                _logger.LogInformation("Todos os mobs foram removidos");
            }
        }

        /// <summary>
        /// Obtém a contagem de mobs
        /// </summary>
        public int MobCount => _mobs.Count;
    }
} 