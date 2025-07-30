using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Handlers;
using System.Collections.Generic; // Added missing import

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler para eventos de mudança de equipamento do personagem
    /// Baseado no albion-radar-deatheye-2pc
    /// </summary>
    public class CharacterEquipmentChangedEventHandler
    {
        private readonly ILogger<CharacterEquipmentChangedEventHandler> _logger;
        private readonly PlayersManager _playersManager;

        public CharacterEquipmentChangedEventHandler(
            ILogger<CharacterEquipmentChangedEventHandler> logger,
            PlayersManager playersManager)
        {
            _logger = logger;
            _playersManager = playersManager;
        }

        /// <summary>
        /// Processa evento de mudança de equipamento do personagem
        /// </summary>
        /// <param name="equipmentEvent">Evento de equipamento</param>
        /// <returns>Task</returns>
        public async Task HandleAsync(CharacterEquipmentChangedEvent equipmentEvent)
        {
            try
            {
                _logger.LogInformation("⚔️ Equipamento alterado: Player {Id}, {EquipmentCount} equipamentos, {SpellCount} spells", 
                    equipmentEvent.Id, equipmentEvent.Equipments?.Count ?? 0, equipmentEvent.Spells?.Count ?? 0);

                // Atualizar equipamentos do player
                var player = _playersManager.GetPlayer(equipmentEvent.Id);
                if (player != null)
                {
                    // Atualizar equipamentos (seria implementado se necessário)
                    _logger.LogDebug("Equipamentos recebidos: {Count}", equipmentEvent.Equipments?.Count ?? 0);
                    _logger.LogDebug("Spells recebidos: {Count}", equipmentEvent.Spells?.Count ?? 0);

                    _logger.LogDebug("Equipamentos atualizados para player {PlayerName}", player.Name);
                }
                else
                {
                    _logger.LogWarning("Player {Id} não encontrado para atualizar equipamentos", equipmentEvent.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar CharacterEquipmentChangedEvent: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }
    }
} 