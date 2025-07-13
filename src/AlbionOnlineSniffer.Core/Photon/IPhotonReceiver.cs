namespace AlbionOnlineSniffer.Core.Photon
{
    public interface IPhotonReceiver
    {
        void ReceivePacket(byte[] payload);
    }
} 