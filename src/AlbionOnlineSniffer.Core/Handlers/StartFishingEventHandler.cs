using Albion.Network;
using Albion.Events.V1;
using AlbionOnlineSniffer.Core.Models.Events;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class StartFishingEventHandler : EventPacketHandler<StartFishingEvent>
    {
        private readonly EventDispatcher eventDispatcher;
        private readonly ILogger<StartFishingEventHandler> logger;

        public StartFishingEventHandler(EventDispatcher eventDispatcher, ILogger<StartFishingEventHandler> logger) 
            : base(PacketIndexesLoader.GlobalPacketIndexes?.StartFishing ?? 0)
        {
            this.eventDispatcher = eventDispatcher;
            this.logger = logger;
        }

        protected override async Task OnActionAsync(StartFishingEvent value)
        {
            try
            {
                // ✅ VALIDAR DADOS DO EVENTO
                if (value == null)
                {
                    logger.LogWarning("StartFishingEvent é null, ignorando...");
                    return;
                }

                // ✅ VALIDAR DADOS DE PESCA
                if (value.RodId <= 0 || value.BaitId <= 0)
                {
                    logger.LogWarning("Dados de pesca inválidos: RodId={RodId}, BaitId={BaitId}", 
                        value.RodId, value.BaitId);
                    return;
                }

                // ✅ VALIDAR COORDENADAS
                if (value.TargetX == 0 && value.TargetY == 0)
                {
                    logger.LogWarning("Coordenadas de pesca inválidas: X={X}, Y={Y}", 
                        value.TargetX, value.TargetY);
                    return;
                }

                logger.LogDebug("Processando evento de início de pesca: RodId={RodId}, BaitId={BaitId}, X={X}, Y={Y}", 
                    value.RodId, value.BaitId, value.TargetX, value.TargetY);

                // 🚀 CRIAR E DESPACHAR EVENTO V1
                var evt = new FishingStartedV1
                {
                    EventId = System.Guid.NewGuid().ToString("n"),
                    ObservedAt = System.DateTimeOffset.UtcNow,
                    RodId = value.RodId,
                    BaitId = value.BaitId,
                    TargetX = value.TargetX,
                    TargetY = value.TargetY
                };

                await eventDispatcher.DispatchEvent(evt);
                logger.LogDebug("Evento FishingStartedV1 despachado com sucesso");
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Erro ao processar StartFishingEvent: {Message}", ex.Message);
                throw;
            }
        }
    }
}


