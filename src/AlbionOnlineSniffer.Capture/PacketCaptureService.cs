using SharpPcap;
using PacketDotNet;
using AlbionOnlineSniffer.Capture.Services;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Capture
{
    using Interfaces;
    public class PacketCaptureService : IPacketCaptureService
    {
        private readonly List<ICaptureDevice> _devices = new List<ICaptureDevice>();
        private bool _isCapturing;
        private readonly string _filter;
        private readonly int _udpPort;
        private readonly PacketCaptureMonitor _monitor;
        private readonly IAlbionEventLogger _eventLogger;

        // Evento para encaminhar o payload UDP ao parser
        public event Action<byte[]>? OnUdpPayloadCaptured;

        /// <summary>
        /// Métricas de captura em tempo real
        /// </summary>
        public PacketCaptureMonitor Monitor => _monitor;

        public PacketCaptureService(int udpPort = 5050, IAlbionEventLogger? eventLogger = null)
        {
            _udpPort = udpPort;
            _filter = $"udp and port {_udpPort}";
            _eventLogger = eventLogger ?? new AlbionOnlineSniffer.Core.Services.AlbionEventLogger();

            // Criar monitor sem logger
            _monitor = new PacketCaptureMonitor();
        }

        private void CheckCaptureDrivers()
        {
            // Verificação silenciosa dos drivers
        }

        public void Start()
        {
            if (_isCapturing)
            {
                return;
            }

            try
            {
                CheckCaptureDrivers();

                var devices = CaptureDeviceList.Instance;
                if (devices.Count == 0)
                {
                    _eventLogger.LogCaptureError("Nenhum adaptador de rede encontrado", "StartCapture");
                    return;
                }

                var validDevices = new List<ICaptureDevice>();

                for (int i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];
                                    var deviceNames = new[] { device.Name, device.Description };
                var deviceName = deviceNames.FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? $"Device_{i}";

                // Adicionar todos os dispositivos válidos
                validDevices.Add(device);
                _eventLogger.LogNetworkDevice(deviceName, device.GetType().Name, true);
                }

                if (validDevices.Count == 0)
                {
                    _eventLogger.LogCaptureError("Nenhum adaptador de rede válido encontrado", "StartCapture");
                    return;
                }

                // Inicia captura em TODAS as interfaces válidas (inclui loopback/lo0)
                foreach (var device in validDevices)
                {
                    try
                    {
                        device.OnPacketArrival += Device_OnPacketArrival;
                        device.Open(DeviceModes.Promiscuous, 5);
                        device.Filter = _filter;
                        device.StartCapture();
                        _devices.Add(device);
                        _isCapturing = true;
                        _monitor.LogCaptureStarted(device.Description, _filter);
                        _monitor.LogFilterApplied(_filter, device.Description);
                    }
                    catch (Exception ex)
                    {
                        _eventLogger.LogCaptureError($"Erro ao iniciar captura no dispositivo {device.Description}", "StartCapture", ex);
                        device.Close();
                    }
                }

                // Registrar quantidade de dispositivos e nomes
                try
                {
                    var deviceNames = validDevices.Select(d => string.IsNullOrWhiteSpace(d.Description) ? d.Name : d.Description).ToArray();
                    _monitor.LogNetworkDevices(validDevices.Count, deviceNames);
                }
                catch { }
            }
            catch (Exception ex)
            {
                _eventLogger.LogCaptureError("Erro geral ao iniciar captura", "StartCapture", ex);
            }
        }

        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var rawPacket = e.GetPacket();
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

                // Tentar extrair o UdpPacket usando métodos reutilizáveis
                var udpPacket = ExtractUdpPacket(packet);

                if (udpPacket != null)
                {
                    ProcessUdpPacket(udpPacket);
                }
            }
            catch (Exception ex)
            {
                _monitor.LogCaptureError(ex, "Device_OnPacketArrival");
                _eventLogger.LogCaptureError("Erro ao processar pacote capturado", "Device_OnPacketArrival", ex);
            }
        }

        /// <summary>
        /// Extrai um UdpPacket de um pacote, independentemente da estrutura
        /// </summary>
        /// <param name="packet">Pacote a ser analisado</param>
        /// <returns>UdpPacket se encontrado, null caso contrário</returns>
        public static UdpPacket? ExtractUdpPacket(Packet packet)
        {
            if (packet == null) return null;

            // Cenário 1: Pacote UDP direto
            if (packet is UdpPacket udpPacket)
            {
                return udpPacket;
            }

            // Cenário 2: Pacote UDP aninhado em Ethernet/IPv4
            if (packet is EthernetPacket ethernetPacket)
            {
                return ExtractUdpPacketFromEthernet(ethernetPacket);
            }

            // Cenário 3: Pacote UDP aninhado em IPv4
            if (packet is IPv4Packet ipv4Packet)
            {
                return ExtractUdpPacketFromIPv4(ipv4Packet);
            }

            // Cenário 4: Verificar se há payload que pode conter UDP
            if (packet.PayloadPacket != null)
            {
                return ExtractUdpPacket(packet.PayloadPacket);
            }

            return null;
        }

        /// <summary>
        /// Extrai UdpPacket de um pacote Ethernet
        /// </summary>
        /// <param name="ethernetPacket">Pacote Ethernet</param>
        /// <returns>UdpPacket se encontrado, null caso contrário</returns>
        public static UdpPacket? ExtractUdpPacketFromEthernet(EthernetPacket ethernetPacket)
        {
            if (ethernetPacket?.PayloadPacket == null) return null;

            // Verificar se o payload é um IPv4Packet
            if (ethernetPacket.PayloadPacket is IPv4Packet ipv4Packet)
            {
                return ExtractUdpPacketFromIPv4(ipv4Packet);
            }

            // Verificar se o payload é um IPv6Packet
            if (ethernetPacket.PayloadPacket is IPv6Packet ipv6Packet)
            {
                return ExtractUdpPacketFromIPv6(ipv6Packet);
            }

            // Verificar se o payload é diretamente um UdpPacket
            if (ethernetPacket.PayloadPacket is UdpPacket udpPacket)
            {
                return udpPacket;
            }

            // Recursão para verificar payloads aninhados
            return ExtractUdpPacket(ethernetPacket.PayloadPacket);
        }

        /// <summary>
        /// Extrai UdpPacket de um pacote IPv4
        /// </summary>
        /// <param name="ipv4Packet">Pacote IPv4</param>
        /// <returns>UdpPacket se encontrado, null caso contrário</returns>
        public static UdpPacket? ExtractUdpPacketFromIPv4(IPv4Packet ipv4Packet)
        {
            if (ipv4Packet?.PayloadPacket == null) return null;

            // Verificar se o payload é um UdpPacket
            if (ipv4Packet.PayloadPacket is UdpPacket udpPacket)
            {
                return udpPacket;
            }

            // Recursão para verificar payloads aninhados
            return ExtractUdpPacket(ipv4Packet.PayloadPacket);
        }

        /// <summary>
        /// Extrai UdpPacket de um pacote IPv6
        /// </summary>
        /// <param name="ipv6Packet">Pacote IPv6</param>
        /// <returns>UdpPacket se encontrado, null caso contrário</returns>
        public static UdpPacket? ExtractUdpPacketFromIPv6(IPv6Packet ipv6Packet)
        {
            if (ipv6Packet?.PayloadPacket == null) return null;

            // Verificar se o payload é um UdpPacket
            if (ipv6Packet.PayloadPacket is UdpPacket udpPacket)
            {
                return udpPacket;
            }

            // Recursão para verificar payloads aninhados
            return ExtractUdpPacket(ipv6Packet.PayloadPacket);
        }

        /// <summary>
        /// Processa um pacote UDP extraído
        /// </summary>
        /// <param name="udpPacket">Pacote UDP a ser processado</param>
        private void ProcessUdpPacket(UdpPacket udpPacket)
        {
            var sourcePort = udpPacket.SourcePort;
            var destPort = udpPacket.DestinationPort;
            var payload = udpPacket.PayloadData;

            if (payload != null && payload.Length > 0)
            {
                // Log do pacote capturado para a interface web
                _eventLogger.LogUdpPacketCapture(
                    payload,
                    "0.0.0.0", // IP origem (não disponível no pacote UDP)
                    sourcePort,
                    "0.0.0.0", // IP destino (não disponível no pacote UDP)
                    destPort
                );

                _monitor.LogPacketCaptured(payload, true);
                OnUdpPayloadCaptured?.Invoke(payload);
            }
            else
            {
                _monitor.LogPacketCaptured(payload ?? Array.Empty<byte>(), false);
            }
        }

        public void Stop()
        {
            if (!_isCapturing)
            {
                return;
            }

            try
            {
                foreach (var device in _devices)
                {
                    try
                    {
                        device.StopCapture();
                        device.Close();
                    }
                    catch { }
                }
                _devices.Clear();
                _isCapturing = false;
                _monitor.LogCaptureStopped();
            }
            catch (Exception ex)
            {
                _eventLogger.LogCaptureError("Erro ao parar captura", "StopCapture", ex);
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();
                _monitor?.Dispose();
            }
            catch (Exception ex)
            {
                // Tratamento silencioso de erro
            }
        }
    }
}
