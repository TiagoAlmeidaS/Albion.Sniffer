using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Handlers;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using AlbionOnlineSniffer.Core.Models.Events;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Processador de pacotes que integra o sistema de offsets com os handlers
    /// Baseado no sistema do albion-radar-deatheye-2pc
    /// </summary>
    public class PacketProcessor
    {
        private readonly ILogger<PacketProcessor> _logger;
        private readonly PacketOffsets _packetOffsets;
        private readonly PlayersManager _playersManager;
        private readonly MobsManager _mobsManager;
        private readonly HarvestablesManager _harvestablesManager;
        private readonly LootChestsManager _lootChestsManager;
        private readonly PositionDecryptor _positionDecryptor;
        private readonly EventDispatcher _eventDispatcher;

        // Mapeamento de IDs de pacotes para tipos (baseado no albion-radar-deatheye-2pc)
        private readonly Dictionary<int, string> _packetIdToType = new()
        {
            { 1, "Leave" },
            { 2, "JoinResponse" },
            { 3, "Move" },
            { 6, "HealthUpdateEvent" },
            { 21, "MoveRequest" },
            { 29, "NewCharacter" },
            { 35, "ChangeCluster" },
            { 39, "NewHarvestableList" },
            { 40, "NewHarvestableObject" },
            { 46, "HarvestableChangeState" },
            { 47, "MobChangeState" },
            { 90, "CharacterEquipmentChanged" },
            { 91, "RegenerationHealthChangedEvent" },
            { 123, "NewMobEvent" },
            { 209, "Mounted" },
            { 280, "LoadClusterObjects" },
            { 319, "NewDungeonExit" },
            { 355, "NewFishingZoneObject" },
            { 359, "ChangeFlaggingFinished" },
            { 387, "NewLootChest" },
            { 514, "MistsPlayerJoinedInfo" },
            { 525, "NewWispGate" },
            { 526, "WispGateOpened" },
            { 593, "KeySync" }
        };

        public PacketProcessor(ILogger<PacketProcessor> logger, PacketOffsets packetOffsets, 
            PlayersManager playersManager, MobsManager mobsManager, 
            HarvestablesManager harvestablesManager, LootChestsManager lootChestsManager,
            PositionDecryptor positionDecryptor, EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _packetOffsets = packetOffsets;
            _playersManager = playersManager;
            _mobsManager = mobsManager;
            _harvestablesManager = harvestablesManager;
            _lootChestsManager = lootChestsManager;
            _positionDecryptor = positionDecryptor;
            _eventDispatcher = eventDispatcher;
        }

        /// <summary>
        /// Processa um pacote enriquecido
        /// </summary>
        /// <param name="enrichedPacket">Pacote enriquecido</param>
        public async Task ProcessPacket(EnrichedPhotonPacket enrichedPacket)
        {
            try
            {
                // Determinar o tipo do pacote
                var packetType = GetPacketType(enrichedPacket.PacketId);
                if (string.IsNullOrEmpty(packetType))
                {
                    _logger.LogDebug("Tipo de pacote desconhecido: ID {PacketId}", enrichedPacket.PacketId);
                    return;
                }

                // Converter parâmetros para o formato esperado pelos handlers
                var parameters = ConvertParameters(enrichedPacket.Parameters);

                // Processar baseado no tipo
                await ProcessPacketByType(packetType, parameters);

                _logger.LogDebug("Pacote processado: {PacketType} (ID: {PacketId})", packetType, enrichedPacket.PacketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Obtém o tipo do pacote baseado no ID
        /// </summary>
        private string GetPacketType(int packetId)
        {
            return _packetIdToType.TryGetValue(packetId, out var packetType) ? packetType : string.Empty;
        }

        /// <summary>
        /// Converte parâmetros do formato string para byte
        /// </summary>
        private Dictionary<byte, object> ConvertParameters(Dictionary<string, object> stringParameters)
        {
            var parameters = new Dictionary<byte, object>();
            
            foreach (var kvp in stringParameters)
            {
                if (byte.TryParse(kvp.Key, out var key))
                {
                    parameters[key] = kvp.Value;
                }
            }

            return parameters;
        }

        /// <summary>
        /// Processa o pacote baseado no tipo
        /// </summary>
        private async Task ProcessPacketByType(string packetType, Dictionary<byte, object> parameters)
        {
            try
            {
                switch (packetType)
                {
                    case "NewCharacter":
                        await _playersManager.ProcessNewCharacter(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("NewCharacter"));
                        break;

                    case "Move":
                        await _playersManager.ProcessMove(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("Move"));
                        break;

                    case "NewMobEvent":
                        await _mobsManager.ProcessNewMob(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("NewMobEvent"));
                        break;

                    case "NewHarvestableObject":
                        await _harvestablesManager.ProcessNewHarvestable(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("NewHarvestableObject"));
                        break;

                    case "NewLootChest":
                        await _lootChestsManager.ProcessNewLootChest(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("NewLootChest"));
                        break;

                    case "Leave":
                        ProcessLeave(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("Leave"));
                        break;

                    case "HealthUpdateEvent":
                        ProcessHealthUpdate(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("HealthUpdateEvent"));
                        break;

                    case "Mounted":
                        ProcessMounted(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("Mounted"));
                        break;

                    case "MobChangeState":
                        ProcessMobChangeState(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("MobChangeState"));
                        break;

                    case "HarvestableChangeState":
                        ProcessHarvestableChangeState(parameters);
                        // Disparar evento para mensageria
                        await _eventDispatcher.DispatchEvent(new GenericGameEvent("HarvestableChangeState"));
                        break;

                    default:
                        _logger.LogDebug("Tipo de pacote não processado: {PacketType}", packetType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pacote do tipo {PacketType}: {Message}", packetType, ex.Message);
            }
        }

        /// <summary>
        /// Processa evento Leave
        /// </summary>
        private void ProcessLeave(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.Leave;
                if (offsets.Length > 0 && parameters.ContainsKey(offsets[0]))
                {
                    var playerId = Convert.ToInt32(parameters[offsets[0]]);
                    _playersManager.RemovePlayer(playerId);
                    _logger.LogInformation("Jogador saiu: ID {PlayerId}", playerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento Leave: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Processa evento HealthUpdateEvent
        /// </summary>
        private void ProcessHealthUpdate(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.HealthUpdateEvent;
                if (offsets.Length >= 2 && parameters.ContainsKey(offsets[0]) && parameters.ContainsKey(offsets[1]))
                {
                    var playerId = Convert.ToInt32(parameters[offsets[0]]);
                    var health = Convert.ToInt32(parameters[offsets[1]]);
                    _playersManager.UpdatePlayerHealth(playerId, health);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento HealthUpdateEvent: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Processa evento Mounted
        /// </summary>
        private void ProcessMounted(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.Mounted;
                if (offsets.Length >= 2 && parameters.ContainsKey(offsets[0]) && parameters.ContainsKey(offsets[1]))
                {
                    var playerId = Convert.ToInt32(parameters[offsets[0]]);
                    var isMounted = Convert.ToBoolean(parameters[offsets[1]]);
                    
                    var player = _playersManager.GetPlayer(playerId);
                    if (player != null)
                    {
                        player.IsMounted = isMounted;
                        _logger.LogDebug("Jogador {PlayerName} montou/desmontou: {IsMounted}", player.Name, isMounted);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento Mounted: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Processa evento MobChangeState
        /// </summary>
        private void ProcessMobChangeState(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.MobChangeState;
                if (offsets.Length >= 2 && parameters.ContainsKey(offsets[0]) && parameters.ContainsKey(offsets[1]))
                {
                    var mobId = Convert.ToInt32(parameters[offsets[0]]);
                    var state = Convert.ToInt32(parameters[offsets[1]]);
                    
                    // Se state = 0, mob morreu/desapareceu
                    if (state == 0)
                    {
                        _mobsManager.RemoveMob(mobId);
                        _logger.LogInformation("Mob removido: ID {MobId}", mobId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento MobChangeState: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Processa evento HarvestableChangeState
        /// </summary>
        private void ProcessHarvestableChangeState(Dictionary<byte, object> parameters)
        {
            try
            {
                var offsets = _packetOffsets.HarvestableChangeState;
                if (offsets.Length >= 3 && parameters.ContainsKey(offsets[0]) && 
                    parameters.ContainsKey(offsets[1]) && parameters.ContainsKey(offsets[2]))
                {
                    var harvestableId = Convert.ToInt32(parameters[offsets[0]]);
                    var count = Convert.ToInt32(parameters[offsets[1]]);
                    var charge = Convert.ToInt32(parameters[offsets[2]]);
                    
                    _harvestablesManager.UpdateHarvestable(harvestableId, count, charge);
                    _logger.LogDebug("Harvestable atualizado: ID {HarvestableId}, Count: {Count}, Charge: {Charge}", 
                        harvestableId, count, charge);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento HarvestableChangeState: {Message}", ex.Message);
            }
        }
    }
} 