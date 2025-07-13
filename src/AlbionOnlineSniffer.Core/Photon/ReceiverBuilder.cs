using System.Collections.Generic;

namespace AlbionOnlineSniffer.Core.Photon
{
    public class ReceiverBuilder
    {
        public static ReceiverBuilder Create() => new ReceiverBuilder();
        public void AddEventHandler(object handler) { }
        public void AddRequestHandler(object handler) { }
        public void AddResponseHandler(object handler) { }
        public IPhotonReceiver Build() => null; // Implemente um stub ou mock se necessário
    }
} 