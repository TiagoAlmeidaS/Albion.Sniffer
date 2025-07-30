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
    /// Gerenciador de jogadores baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class PlayersManager
    {
        private readonly ILogger<PlayersManager> _logger;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly ConcurrentDictionary<int, Player> _players = new();
        private readonly NewCharacterEventHandler _newCharacterHandler;
        private readonly MoveEventHandler _moveHandler;
        private readonly EventDispatcher _eventDispatcher;

        public PlayersManager(ILogger<PlayersManager> logger, PositionDecryptor positionDecryptor, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _positionDecryptor = positionDecryptor;
            _eventDispatcher = eventDispatcher;
            
            // Criar loggers específicos para os handlers
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var newCharacterLogger = loggerFactory.CreateLogger<NewCharacterEventHandler>();
            var moveLogger = loggerFactory.CreateLogger<MoveEventHandler>();
            
            _newCharacterHandler = new NewCharacterEventHandler(newCharacterLogger, positionDecryptor);
            _moveHandler = new MoveEventHandler(moveLogger, positionDecryptor);
        }

        /// <summary>
        /// Adiciona um novo jogador
        /// </summary>
        public void AddPlayer(int id, string name, string guild, string alliance, Vector2 position, Health health, Faction faction, Equipment equipment, int[] spells)
        {
            lock (_players)
            {
                if (_players.ContainsKey(id))
                {
                    _players.TryRemove(id, out Player? existingPlayer);
                }

                var player = new Player(id, name, guild, alliance, position, health, faction, equipment, spells);
                _players.TryAdd(id, player);
                
                _logger.LogInformation("Jogador adicionado: {Name} (ID: {Id})", name, id);

                // Disparar evento de jogador detectado
                _ = _eventDispatcher.DispatchEvent(new PlayerDetectedEvent(player));
            }
        }

        /// <summary>
        /// Remove um jogador
        /// </summary>
        public void RemovePlayer(int id)
        {
            lock (_players)
            {
                if (_players.TryRemove(id, out Player? player))
                {
                    _logger.LogInformation("Jogador removido: {Name} (ID: {Id})", player?.Name ?? "Unknown", id);
                    
                    // Disparar evento de jogador saiu
                    _ = _eventDispatcher.DispatchEvent(new PlayerLeftEvent(id));
                }
            }
        }

        /// <summary>
        /// Atualiza a posição de um jogador
        /// </summary>
        public void UpdatePlayerPosition(int id, Vector2 position)
        {
            lock (_players)
            {
                if (_players.TryGetValue(id, out Player? player))
                {
                    player.Position = position;
                    player.Time = DateTime.UtcNow;
                    
                    // Disparar evento de movimento
                    _ = _eventDispatcher.DispatchEvent(new PlayerMovedEvent(id, position));
                }
            }
        }

        /// <summary>
        /// Atualiza a saúde de um jogador
        /// </summary>
        public void UpdatePlayerHealth(int id, int health)
        {
            lock (_players)
            {
                if (_players.TryGetValue(id, out Player? player))
                {
                    player.Health.Value = health;
                }
            }
        }

        /// <summary>
        /// Processa um evento NewCharacter
        /// </summary>
        public async Task ProcessNewCharacter(Dictionary<byte, object> parameters)
        {
            var player = await _newCharacterHandler.HandleNewCharacter(parameters);
            if (player is not null)
            {
                AddPlayer(player.Id, player.Name, player.Guild, player.Alliance, 
                         player.Position, player.Health, player.Faction, player.Equipment, player.Spells);
            }
        }

        /// <summary>
        /// Processa um evento Move
        /// </summary>
        public async Task ProcessMove(Dictionary<byte, object> parameters)
        {
            var moveData = await _moveHandler.HandleMove(parameters);
            if (moveData != null)
            {
                UpdatePlayerPosition(moveData.PlayerId, moveData.Position);
            }
        }

        /// <summary>
        /// Obtém todos os jogadores
        /// </summary>
        public IEnumerable<Player> GetAllPlayers()
        {
            return _players.Values;
        }

        /// <summary>
        /// Obtém um jogador específico
        /// </summary>
        public Player? GetPlayer(int id)
        {
            _players.TryGetValue(id, out Player? player);
            return player;
        }

        /// <summary>
        /// Limpa todos os jogadores
        /// </summary>
        public void Clear()
        {
            lock (_players)
            {
                _players.Clear();
                _logger.LogInformation("Todos os jogadores foram removidos");
            }
        }

        /// <summary>
        /// Obtém a contagem de jogadores
        /// </summary>
        public int PlayerCount => _players.Count;
    }
} 