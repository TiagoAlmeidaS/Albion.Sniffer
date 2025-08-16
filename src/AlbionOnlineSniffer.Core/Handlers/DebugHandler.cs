using Albion.Network;

namespace AlbionOnlineSniffer.Core.Handlers
{
    public class DebugHandler : PacketHandler<object>
    {
        protected override Task OnHandleAsync(object packet)
        {
            // Processamento silencioso dos pacotes - logs serão exibidos na interface web
            if (packet is ResponsePacket response)
            {
                if (response.Parameters.TryGetValue(253, out var code))
                {
                    // Processar response packet
                }
            }
            else if (packet is RequestPacket request)
            {
                if (request.Parameters.TryGetValue(253, out var code))
                {
                    // Processar request packet
                }
            }
            else if (packet is EventPacket @event)
            {
                if (@event.Parameters.TryGetValue(252, out var code))
                {
                    // Processar event packet
                }
            }
            
            return Task.CompletedTask;
        }
    }
}