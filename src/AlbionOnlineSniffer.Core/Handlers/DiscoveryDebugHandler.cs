using Albion.Network;
using AlbionOnlineSniffer.Core.Models.Discovery;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlbionOnlineSniffer.Core.Handlers
{
    /// <summary>
    /// Handler universal para interceptação de pacotes descriptografados pelo Albion.Network
    /// Permite descoberta automática de novos offsets sem interferir no fluxo existente
    /// </summary>
    public class DiscoveryDebugHandler : PacketHandler<object>
    {
        private readonly ILogger<DiscoveryDebugHandler> _logger;
        
        // ✅ EVENTO para interceptação de descoberta (estático para acesso global)
        public static event Action<DecryptedPacketData>? OnPacketDecrypted;
        
        public DiscoveryDebugHandler(ILogger<DiscoveryDebugHandler> logger)
        {
            _logger = logger;
        }
        
        protected override Task OnHandleAsync(object packet)
        {
            try
            {
                // ✅ INTERCEPTAÇÃO UNIVERSAL - Todos os pacotes descriptografados
                var decryptedData = CreateDecryptedPacketData(packet);
                
                // ✅ DISPARAR EVENTO DE DESCOBERTA SEM BLOQUEAR
                OnPacketDecrypted?.Invoke(decryptedData);
                
                // ✅ LOG DE DEBUG (apenas em desenvolvimento)
                _logger.LogDebug("🔍 Pacote interceptado para descoberta: {Type} - {Code}", 
                    decryptedData.PacketType, decryptedData.PacketCode);
                
                // ✅ LOG MAIS VISÍVEL PARA DEBUG
                _logger.LogInformation("🔍 DISCOVERY: Pacote interceptado: {Type} - {Code} - {EventName}", 
                    decryptedData.PacketType, decryptedData.PacketCode, decryptedData.EventName);
                
                // ✅ PUBLICAR NO EVENTDISPATCHER PARA USAR A ESTRUTURA EXISTENTE
                // O EventToQueueBridge já existente irá capturar e enviar para fila albion.discovery.raw
                _logger.LogInformation("🔗 DiscoveryDebugHandler: Pacote será processado pelo EventToQueueBridge existente");
            }
            catch (Exception ex)
            {
                // ✅ TRATAMENTO SILENCIOSO - NÃO AFETA FLUXO PRINCIPAL
                _logger.LogWarning("⚠️ Erro na interceptação de descoberta: {Message}", ex.Message);
            }
            
            // ✅ RETORNO IMEDIATO - NÃO BLOQUEIA FLUXO
            return Task.CompletedTask;
        }
        
        private DecryptedPacketData CreateDecryptedPacketData(object packet)
        {
            var decryptedData = new DecryptedPacketData
            {
                EventName = GetPacketType(packet),
                RawPacket = packet,
                Timestamp = DateTime.UtcNow,
                IsDecrypted = true,
                PacketType = packet.GetType().Name
            };
            
            // ✅ EXTRAIR PARÂMETROS BASEADO NO TIPO DE PACOTE
            if (packet is ResponsePacket response)
            {
                decryptedData.Parameters["Code"] = response.Parameters.TryGetValue(253, out var code) ? code : "unknown";
                decryptedData.Parameters["Parameters"] = response.Parameters;
                decryptedData.PacketCode = response.Parameters.TryGetValue(253, out var codeValue) ? Convert.ToInt32(codeValue) : null;
                decryptedData.ParameterCount = response.Parameters.Count;
            }
            else if (packet is RequestPacket request)
            {
                decryptedData.Parameters["Code"] = request.Parameters.TryGetValue(253, out var code) ? code : "unknown";
                decryptedData.Parameters["Parameters"] = request.Parameters;
                decryptedData.PacketCode = request.Parameters.TryGetValue(253, out var codeValue) ? Convert.ToInt32(codeValue) : null;
                decryptedData.ParameterCount = request.Parameters.Count;
            }
            else if (packet is EventPacket @event)
            {
                decryptedData.Parameters["Code"] = @event.Parameters.TryGetValue(252, out var code) ? code : "unknown";
                decryptedData.Parameters["Parameters"] = @event.Parameters;
                decryptedData.PacketCode = @event.Parameters.TryGetValue(252, out var codeValue) ? Convert.ToInt32(codeValue) : null;
                decryptedData.ParameterCount = @event.Parameters.Count;
            }
            else
            {
                // ✅ PACOTE DESCONHECIDO - AINDA INTERCEPTA
                decryptedData.Parameters["RawType"] = packet.GetType().FullName;
                decryptedData.Parameters["ToString"] = packet.ToString() ?? "null";
            }
            
            return decryptedData;
        }
        
        private string GetPacketType(object packet)
        {
            return packet switch
            {
                ResponsePacket => "ResponsePacket",
                RequestPacket => "RequestPacket", 
                EventPacket => "EventPacket",
                _ => packet.GetType().Name
            };
        }
    }
}
