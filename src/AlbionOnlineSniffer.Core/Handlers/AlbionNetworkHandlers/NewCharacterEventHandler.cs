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
    /// Handler para eventos NewCharacter que implementa Albion.Network.EventPacketHandler<T>
    /// Conecta com o EventDispatcher para publicação na fila
    /// </summary>
    public class NewCharacterEventHandler : EventPacketHandler<AlbionNetworkNewCharacterEvent>
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<NewCharacterEventHandler> _logger;

        public NewCharacterEventHandler(EventDispatcher eventDispatcher, ILogger<NewCharacterEventHandler> logger, PacketIndexes packetIndexes) 
            : base(packetIndexes.NewCharacter)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
            _logger.LogInformation("🔧 NewCharacterEventHandler criado com PacketIndex: {Index}", packetIndexes.NewCharacter);
        }

        protected override async Task OnActionAsync(AlbionNetworkNewCharacterEvent value)
        {
            try
            {
                _logger.LogInformation("🎯 PROCESSANDO NewCharacterEvent: {Name} (ID: {Id})", 
                    value.Name, value.Id);

                // Criar Player para o construtor do NewCharacterEvent
                var player = new Player(
                    value.Id, 
                    value.Name, 
                    value.Guild, 
                    value.Alliance, 
                    value.Position, 
                    value.Health, 
                    value.Faction, 
                    new Equipment(), // TODO: Converter equipments
                    value.Spells ?? Array.Empty<int>()
                );

                // Criar evento do jogo para o EventDispatcher
                var gameEvent = new NewCharacterEvent(player);
                gameEvent.Timestamp = DateTime.UtcNow;

                // Disparar para o EventDispatcher que publicará na fila
                await _eventDispatcher.DispatchEvent(gameEvent);

                _logger.LogInformation("✅ NewCharacterEvent processado e disparado para fila: {Name}", value.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar NewCharacterEvent: {Message}", ex.Message);
            }
        }
    }
} 
