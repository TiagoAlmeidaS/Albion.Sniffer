using System;
using System.Threading.Tasks;
using Albion.Network;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Models;
using AlbionOnlineSniffer.Core.Models.GameObjects;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Handlers.AlbionNetworkHandlers
{
    /// <summary>
    /// Handler para eventos CharacterEquipmentChanged que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class CharacterEquipmentChangedEventHandler : EventPacketHandler<AlbionNetworkCharacterEquipmentChangedEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<CharacterEquipmentChangedEventHandler> _logger;

        public CharacterEquipmentChangedEventHandler(EventDispatcher eventDispatcher, ILogger<CharacterEquipmentChangedEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.CharacterEquipmentChanged)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        protected override async Task OnActionAsync(AlbionNetworkCharacterEquipmentChangedEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO CharacterEquipmentChangedEvent: ID {Id}, Equipment {EquipmentId}", 
                    value.Id, value.EquipmentId);

                // Criar evento do jogo para o EventDispatcher
                var equipments = new List<Equipment>(); // TODO: Converter equipmentId para Equipment
                var spells = new List<string>(); // TODO: Extrair spells do evento
                var gameEvent = new CharacterEquipmentChangedEvent(value.Id, equipments, spells);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ CharacterEquipmentChangedEvent processado e disparado para fila: ID {Id}", value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar CharacterEquipmentChangedEvent: {Message}", ex.Message);
            }
        }
    }
} 
