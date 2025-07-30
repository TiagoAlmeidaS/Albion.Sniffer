using SharpPcap;
using PacketDotNet;
using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

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

        public PacketCaptureService(int udpPort = 5056) // Corrigido para porta 5056 (porta oficial do Albion Online)
        {
            _udpPort = udpPort;
            _filter = $"udp and port {_udpPort}";
        }

        private void CheckCaptureDrivers()
        {
            Console.WriteLine("Verificando drivers de captura de pacotes...");
            
            // Verificar se estamos no Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Sistema operacional: Windows");
                Console.WriteLine("IMPORTANTE: Para captura de pacotes no Windows, você precisa:");
                Console.WriteLine("1. Instalar Npcap (https://npcap.com/) ou WinPcap");
                Console.WriteLine("2. Executar o programa como Administrador");
                Console.WriteLine("3. Verificar se o firewall não está bloqueando");
            }
        }

        public void Start()
        {
            CheckCaptureDrivers();
            
            var devices = CaptureDeviceList.Instance;
            Console.WriteLine($"Dispositivos disponíveis: {devices.Count}");
            
            if (devices.Count <= 0)
            {
                Console.WriteLine("ERRO: Nenhum adaptador de rede encontrado.");
                Console.WriteLine("Possíveis causas:");
                Console.WriteLine("- Npcap/WinPcap não está instalado");
                Console.WriteLine("- Programa não está sendo executado como Administrador");
                Console.WriteLine("- Firewall bloqueando acesso aos adaptadores");
                throw new InvalidOperationException("No network adapters available for capture.");
            }

            // Listar todos os dispositivos para debug
            for (int i = 0; i < devices.Count; i++)
            {
                var dev = devices[i];
                Console.WriteLine($"Dispositivo {i}: {dev.Description} ({dev.Name})");
                if (dev is SharpPcap.LibPcap.LibPcapLiveDevice liveDev)
                {
                    if (liveDev.Addresses?.Count > 0)
                    {
                        foreach (var addr in liveDev.Addresses)
                        {
                            var ip = addr.Addr?.ipAddress?.ToString() ?? "sem IP";
                            var mask = addr.Netmask?.ipAddress?.ToString() ?? "sem máscara";
                            Console.WriteLine($"    - {ip} / {mask}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  - Nenhum endereço associado.");
                    }
                }

            }

            // Estratégia baseada no albion-radar-deatheye-2pc: capturar todos os dispositivos válidos
            var validDevices = devices.Where(d => 
            {
                // Incluir dispositivos com MAC address ou loopback (como no albion-radar-deatheye-2pc)
                if (d.MacAddress != null)
                {
                    Console.WriteLine($"  - Dispositivo válido (MAC): {d.Description}");
                    return true;
                }
                
                // Capturar loopback também (como no albion-radar-deatheye-2pc)
                if (d is SharpPcap.LibPcap.LibPcapLiveDevice liveDev)
                {
                    var hasLoopback = liveDev.Addresses?.Any(e => e.Addr.ipAddress?.Equals(IPAddress.Loopback) ?? false) ?? false;
                    if (hasLoopback)
                    {
                        Console.WriteLine($"  - Dispositivo loopback: {d.Description}");
                        return true;
                    }
                }
                
                Console.WriteLine($"  - Dispositivo inválido: {d.Description}");
                return false;
            }).ToList();

            if (!validDevices.Any())
            {
                Console.WriteLine("ERRO: Nenhum adaptador de rede válido encontrado.");
                throw new InvalidOperationException("No valid network adapters found.");
            }

            // Iniciar captura em todos os dispositivos válidos (como no albion-radar-deatheye-2pc)
            foreach (var device in validDevices)
            {
                try
                {
                    Console.WriteLine($"Iniciando captura no dispositivo: {device.Description}");
                    StartCaptureOnDevice(device);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRO ao iniciar captura no dispositivo {device.Description}: {ex.Message}");
                }
            }
        }

        private void StartCaptureOnDevice(ICaptureDevice device)
        {
            if (!device.Started)
            {
                device.Open(new DeviceConfiguration
                {
                    Mode = DeviceModes.DataTransferUdp | DeviceModes.Promiscuous | DeviceModes.MaxResponsiveness,
                    ReadTimeout = 5
                });

                device.Filter = _filter;
                device.OnPacketArrival += Device_OnPacketArrival;
                device.StartCapture();
                _isCapturing = true;
                
                Console.WriteLine($"Captura iniciada no dispositivo: {device.Description}");
                Console.WriteLine($"Filtro aplicado: {_filter}");
            }
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