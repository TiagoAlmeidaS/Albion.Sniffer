using SharpPcap;
using PacketDotNet;
using System;
using System.Linq;
using System.Net;

namespace AlbionOnlineSniffer.Capture
{
    using AlbionOnlineSniffer.Capture.Interfaces;
    public class PacketCaptureService : IPacketCaptureService
    {
        private ICaptureDevice? _device;
        private bool _isCapturing;
        private readonly string _filter;
        private readonly int _udpPort;

        // Evento para encaminhar o payload UDP ao parser
        public event Action<byte[]>? OnUdpPayloadCaptured;

        public PacketCaptureService(int udpPort = 5056)
        {
            _udpPort = udpPort;
            _filter = $"udp and port {_udpPort}";
        }

        public void Start()
        {
            var devices = CaptureDeviceList.Instance;
            if (devices.Count <= 0)
                throw new InvalidOperationException("No network adapters available for capture.");

            // Seleciona o primeiro dispositivo válido (pode ser customizável depois)
            var device = devices.FirstOrDefault(d => d.MacAddress != null ||
                ((d as SharpPcap.LibPcap.LibPcapLiveDevice)?.Addresses?.Any(e => e.Addr.ipAddress?.Equals(IPAddress.Loopback) ?? false) ?? false));

            if (device == null)
                throw new InvalidOperationException("No suitable network adapter found.");

            _device = device;
            _device.OnPacketArrival += Device_OnPacketArrival;
            _device.Open(new DeviceConfiguration {
                Mode = DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.MaxResponsiveness,
                ReadTimeout = 5
            });
            _device.Filter = _filter;
            _device.StartCapture();
            _isCapturing = true;
        }

        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var rawPacket = e.GetPacket();
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data).Extract<UdpPacket>();
                if (packet != null && packet.PayloadData != null)
                {
                    OnUdpPayloadCaptured?.Invoke(packet.PayloadData);
                }
            }
            catch (Exception ex)
            {
                // TODO: Adicionar logging
                Console.Error.WriteLine($"[PacketCaptureService] Error parsing packet: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (_isCapturing && _device != null)
            {
                _device.StopCapture();
                _device.Close();
                _isCapturing = false;
            }
        }

        public void Dispose()
        {
            Stop();
            if (_device != null)
            {
                _device.OnPacketArrival -= Device_OnPacketArrival;
                _device = null;
            }
        }
    }
} 