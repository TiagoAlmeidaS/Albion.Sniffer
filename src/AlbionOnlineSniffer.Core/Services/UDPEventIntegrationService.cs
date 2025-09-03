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
                
                // ✅ EXTRAIR INFORMAÇÕES DETALHADAS DO EVENTO PARA DEPURAÇÃO
                var eventDetails = ExtractEventDetails(gameEvent);
                
                // ✅ REGISTRAR EVENTO COMO BEM-SUCEDIDO
                _udpStatistics.RecordEvent(
                    eventType,
                    eventCategory,
                    true, // Assumimos sucesso se chegou até aqui
                    new Dictionary<string, object>
                    {
                        ["timestamp"] = DateTimeOffset.UtcNow,
                        ["eventType"] = eventType,
                        ["category"] = eventCategory,
                        ["details"] = eventDetails
                    }
                );

                _logger.LogInformation("🌐 Evento UDP registrado: {EventType} - {Category} - Params: {ParamCount} - Opcode: {Opcode}", 
                    eventType, eventCategory, eventDetails.ParamCount, eventDetails.Opcode);
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
        /// Extrai detalhes do evento para depuração e análise
        /// </summary>
        private EventDetails ExtractEventDetails(object gameEvent)
        {
            var details = new EventDetails();
            
            try
            {
                // Tentar extrair informações comuns dos eventos
                var eventType = gameEvent.GetType();
                
                // Verificar se tem propriedade PacketCode (opcode)
                var packetCodeProperty = eventType.GetProperty("PacketCode");
                if (packetCodeProperty != null)
                {
                    details.Opcode = packetCodeProperty.GetValue(gameEvent)?.ToString() ?? "N/A";
                }
                
                // Verificar se tem propriedade Parameters
                var parametersProperty = eventType.GetProperty("Parameters");
                if (parametersProperty != null)
                {
                    var parameters = parametersProperty.GetValue(gameEvent);
                    if (parameters is System.Collections.ICollection collection)
                    {
                        details.ParamCount = collection.Count;
                        details.HasParameters = collection.Count > 0;
                    }
                }
                
                // Verificar se tem propriedade Name
                var nameProperty = eventType.GetProperty("Name");
                if (nameProperty != null)
                {
                    details.EventName = nameProperty.GetValue(gameEvent)?.ToString() ?? "N/A";
                }
                
                // Verificar se tem propriedade Timestamp
                var timestampProperty = eventType.GetProperty("Timestamp");
                if (timestampProperty != null)
                {
                    details.Timestamp = timestampProperty.GetValue(gameEvent)?.ToString() ?? "N/A";
                }
                
                _logger.LogDebug("🔍 Detalhes extraídos do evento {EventType}: Opcode={Opcode}, Params={ParamCount}, Name={EventName}", 
                    eventType.Name, details.Opcode, details.ParamCount, details.EventName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ Erro ao extrair detalhes do evento: {Error}", ex.Message);
                details.Error = ex.Message;
            }
            
            return details;
        }

        /// <summary>
        /// Classe para armazenar detalhes do evento
        /// </summary>
        private class EventDetails
        {
            public string Opcode { get; set; } = "N/A";
            public int ParamCount { get; set; } = 0;
            public bool HasParameters { get; set; } = false;
            public string EventName { get; set; } = "N/A";
            public string Timestamp { get; set; } = "N/A";
            public string Error { get; set; } = null;
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
