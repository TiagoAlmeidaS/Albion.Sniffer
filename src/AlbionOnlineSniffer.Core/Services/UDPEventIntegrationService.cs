using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Serviço que integra o UDPStatistics ao EventDispatcher para registrar automaticamente eventos UDP
    /// </summary>
    public class UDPEventIntegrationService : IDisposable
    {
        private readonly ILogger<UDPEventIntegrationService> _logger;
        private readonly EventDispatcher _eventDispatcher;
        private readonly UDPStatistics _udpStatistics;
        private bool _disposed = false;

        public UDPEventIntegrationService(
            ILogger<UDPEventIntegrationService> logger,
            EventDispatcher eventDispatcher,
            UDPStatistics udpStatistics)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            _udpStatistics = udpStatistics;

            // ✅ REGISTRAR HANDLER GLOBAL PARA INTERCEPTAR TODOS OS EVENTOS UDP
            _eventDispatcher.RegisterGlobalHandler(RecordUDPEvent);

            _logger.LogInformation("🌐 UDPEventIntegrationService configurado e conectado ao EventDispatcher");
            _logger.LogInformation("📊 Estatísticas UDP em tempo real ativadas");
        }

        /// <summary>
        /// Handler global que registra todos os eventos UDP no sistema de estatísticas
        /// </summary>
        private async Task RecordUDPEvent(object gameEvent)
        {
            try
            {
                var eventType = gameEvent.GetType().Name;
                var eventCategory = GetEventCategory(eventType);
                
                // ✅ REGISTRAR EVENTO COMO BEM-SUCEDIDO
                _udpStatistics.RecordEvent(
                    eventType,
                    eventCategory,
                    true, // Assumimos sucesso se chegou até aqui
                    new Dictionary<string, object>
                    {
                        ["timestamp"] = DateTimeOffset.UtcNow,
                        ["eventType"] = eventType,
                        ["category"] = eventCategory
                    }
                );

                _logger.LogDebug("🌐 Evento UDP registrado: {EventType} - {Category}", eventType, eventCategory);
            }
            catch (Exception ex)
            {
                // ✅ REGISTRAR EVENTO COMO FALHA
                var eventType = gameEvent.GetType().Name;
                _udpStatistics.RecordEvent(
                    eventType,
                    "Error",
                    false,
                    new Dictionary<string, object>
                    {
                        ["error"] = ex.Message,
                        ["timestamp"] = DateTimeOffset.UtcNow
                    }
                );

                _logger.LogWarning("⚠️ Erro ao registrar evento UDP: {EventType} - {Error}", eventType, ex.Message);
            }
        }

        /// <summary>
        /// Determina a categoria do evento baseado no nome
        /// </summary>
        private string GetEventCategory(string eventType)
        {
            return eventType switch
            {
                var name when name.Contains("Fishing") => "Fishing",
                var name when name.Contains("Dungeon") || name.Contains("Wisp") => "Dungeons",
                var name when name.Contains("Harvestable") => "Harvestables",
                var name when name.Contains("Mob") => "Mobs",
                var name when name.Contains("Character") || name.Contains("Player") || name.Contains("Move") || name.Contains("Health") || name.Contains("Equipment") || name.Contains("Regeneration") || name.Contains("Mists") => "Players",
                var name when name.Contains("Loot") => "Loot",
                var name when name.Contains("Key") || name.Contains("Cluster") || name.Contains("Flagging") => "System",
                _ => "Unknown"
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _logger.LogInformation("🌐 UDPEventIntegrationService finalizado");
            }
        }
    }
}
