using SharpPcap;
using PacketDotNet;
using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using AlbionOnlineSniffer.Capture.Services;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Capture
{
    using AlbionOnlineSniffer.Capture.Interfaces;
    public class PacketCaptureService : IPacketCaptureService
    {
        private ICaptureDevice? _device;
        private bool _isCapturing;
        private readonly string _filter;
        private readonly int _udpPort;
        private readonly PacketCaptureMonitor _monitor;
        private readonly ILogger<PacketCaptureService> _logger;

        // Evento para encaminhar o payload UDP ao parser
        public event Action<byte[]>? OnUdpPayloadCaptured;
        
        /// <summary>
        /// Métricas de captura em tempo real
        /// </summary>
        public PacketCaptureMonitor Monitor => _monitor;

        public PacketCaptureService(int udpPort = 5050, ILogger<PacketCaptureService>? logger = null) // Mantendo porta 5050 conforme especificação do projeto
        {
            _udpPort = udpPort;
            _filter = $"udp and port {_udpPort}";
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PacketCaptureService>.Instance;
            
            // Criar logger para o monitor
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var monitorLogger = loggerFactory.CreateLogger<PacketCaptureMonitor>();
            _monitor = new PacketCaptureMonitor(monitorLogger);
            
            _logger.LogInformation("PacketCaptureService inicializado - Porta: {Port}, Filtro: {Filter}", _udpPort, _filter);
        }

        private void CheckCaptureDrivers()
        {
            _logger.LogInformation("Verificando drivers de captura de pacotes...");
            
            // Verificar se estamos no Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogInformation("Sistema operacional: Windows");
                _logger.LogWarning("IMPORTANTE: Para captura de pacotes no Windows, você precisa:");
                _logger.LogWarning("1. Instalar Npcap (https://npcap.com/) ou WinPcap");
                _logger.LogWarning("2. Executar o programa como Administrador");
                _logger.LogWarning("3. Verificar se o firewall não está bloqueando");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _logger.LogInformation("Sistema operacional: Linux");
                _logger.LogInformation("Certifique-se de que libpcap está instalado e você tem permissões adequadas");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _logger.LogInformation("Sistema operacional: macOS");
                _logger.LogInformation("Certifique-se de que libpcap está disponível");
            }
        }

        public void Start()
        {
            try
            {
                CheckCaptureDrivers();
                
                var devices = CaptureDeviceList.Instance;
                _logger.LogInformation("Dispositivos de rede disponíveis: {DeviceCount}", devices.Count);
                
                if (devices.Count <= 0)
                {
                    _logger.LogError("ERRO: Nenhum adaptador de rede encontrado.");
                    _logger.LogError("Possíveis causas:");
                    _logger.LogError("- Npcap/WinPcap não está instalado");
                    _logger.LogError("- Programa não está sendo executado como Administrador");
                    _logger.LogError("- Firewall bloqueando acesso aos adaptadores");
                    throw new InvalidOperationException("No network adapters available for capture.");
                }

                // Preparar lista de nomes de dispositivos para logging
                var deviceNames = new string[devices.Count];
                
                // Listar todos os dispositivos para debug
                for (int i = 0; i < devices.Count; i++)
                {
                    var dev = devices[i];
                    deviceNames[i] = $"{dev.Description} ({dev.Name})";
                    _logger.LogDebug("Dispositivo {Index}: {DeviceName}", i, deviceNames[i]);
                    
                    if (dev is SharpPcap.LibPcap.LibPcapLiveDevice liveDev)
                    {
                        if (liveDev.Addresses?.Count > 0)
                        {
                            foreach (var addr in liveDev.Addresses)
                            {
                                var ip = addr.Addr?.ipAddress?.ToString() ?? "sem IP";
                                var mask = addr.Netmask?.ipAddress?.ToString() ?? "sem máscara";
                                _logger.LogTrace("    Endereço: {IP} / {Mask}", ip, mask);
                            }
                        }
                        else
                        {
                            _logger.LogTrace("  - Nenhum endereço associado.");
                        }
                    }
                }
                
                // Registrar dispositivos no monitor
                _monitor.LogNetworkDevices(devices.Count, deviceNames);

                // Estratégia baseada no albion-radar-deatheye-2pc: capturar todos os dispositivos válidos
                var validDevices = devices.Where(d => 
                {
                    // Incluir dispositivos com MAC address ou loopback (como no albion-radar-deatheye-2pc)
                    if (d.MacAddress != null)
                    {
                        _logger.LogDebug("Dispositivo válido (MAC): {DeviceName}", d.Description);
                        return true;
                    }
                    
                    // Capturar loopback também (como no albion-radar-deatheye-2pc)
                    if (d is SharpPcap.LibPcap.LibPcapLiveDevice liveDev)
                    {
                        var hasLoopback = liveDev.Addresses?.Any(e => e.Addr.ipAddress?.Equals(IPAddress.Loopback) ?? false) ?? false;
                        if (hasLoopback)
                        {
                            _logger.LogDebug("Dispositivo loopback: {DeviceName}", d.Description);
                            return true;
                        }
                    }
                    
                    _logger.LogTrace("Dispositivo inválido: {DeviceName}", d.Description);
                    return false;
                }).ToList();

                if (!validDevices.Any())
                {
                    _logger.LogError("ERRO: Nenhum adaptador de rede válido encontrado.");
                    throw new InvalidOperationException("No valid network adapters found.");
                }

                _logger.LogInformation("Iniciando captura em {ValidDeviceCount} dispositivos válidos", validDevices.Count);

                // Iniciar captura em todos os dispositivos válidos (como no albion-radar-deatheye-2pc)
                foreach (var device in validDevices)
                {
                    try
                    {
                        _logger.LogInformation("Iniciando captura no dispositivo: {DeviceName}", device.Description);
                        StartCaptureOnDevice(device);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ERRO ao iniciar captura no dispositivo {DeviceName}: {ErrorMessage}", 
                            device.Description, ex.Message);
                        _monitor.LogCaptureError(ex, $"Dispositivo: {device.Description}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro geral ao iniciar captura de pacotes");
                _monitor.LogCaptureError(ex, "Start method");
                throw;
            }
        }

        private void StartCaptureOnDevice(ICaptureDevice device)
        {
            if (!device.Started)
            {
                try
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
                    
                    _logger.LogInformation("Captura iniciada no dispositivo: {DeviceName}", device.Description);
                    _monitor.LogFilterApplied(_filter, device.Description);
                    _monitor.LogCaptureStarted(device.Description, _filter);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao configurar dispositivo {DeviceName}", device.Description);
                    _monitor.LogCaptureError(ex, $"StartCaptureOnDevice: {device.Description}");
                    throw;
                }
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
                    // Registrar pacote válido no monitor
                    _monitor.LogPacketCaptured(packet.PayloadData, isValid: true);
                    
                    // Log detalhado do pacote (apenas em nível Debug/Trace)
                    _logger.LogDebug("Pacote UDP capturado - Porta origem: {SourcePort}, Porta destino: {DestPort}, Tamanho: {Size} bytes",
                        packet.SourcePort, packet.DestinationPort, packet.PayloadData.Length);
                    
                    // Encaminhar para o parser
                    OnUdpPayloadCaptured?.Invoke(packet.PayloadData);
                }
                else
                {
                    // Registrar pacote inválido
                    _monitor.LogPacketCaptured(rawPacket.Data, isValid: false);
                    _logger.LogTrace("Pacote descartado - não é UDP válido ou sem payload");
                }
            }
            catch (Exception ex)
            {
                _monitor.LogCaptureError(ex, "Device_OnPacketArrival");
                _logger.LogError(ex, "Erro ao processar pacote capturado: {ErrorMessage}", ex.Message);
            }
        }

        public void Stop()
        {
            if (_isCapturing && _device != null)
            {
                try
                {
                    _device.StopCapture();
                    _device.Close();
                    _isCapturing = false;
                    
                    _logger.LogInformation("Captura de pacotes parada");
                    _monitor.LogCaptureStopped();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao parar captura de pacotes");
                    _monitor.LogCaptureError(ex, "Stop method");
                }
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();
                if (_device != null)
                {
                    _device.OnPacketArrival -= Device_OnPacketArrival;
                    _device = null;
                }
                
                _monitor?.Dispose();
                _logger.LogInformation("PacketCaptureService finalizado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar PacketCaptureService");
            }
        }
        
        /// <summary>
        /// Força uma atualização imediata das métricas de monitoramento
        /// </summary>
        public void UpdateMetrics()
        {
            _monitor.ForceMetricsUpdate();
        }
        
        /// <summary>
        /// Obtém um resumo detalhado das métricas atuais
        /// </summary>
        public void LogDetailedMetrics()
        {
            _monitor.LogDetailedMetrics();
        }
    }
} 