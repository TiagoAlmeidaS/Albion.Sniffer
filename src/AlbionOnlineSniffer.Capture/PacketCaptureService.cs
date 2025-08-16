using SharpPcap;
using PacketDotNet;
using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using AlbionOnlineSniffer.Capture.Services;
using AlbionOnlineSniffer.Core.Interfaces;
using System.Collections.Generic; // Added for List

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

                foreach (var device in validDevices)
                {
                    try
                    {
                        device.OnPacketArrival += Device_OnPacketArrival;
                        device.Open(DeviceModes.Promiscuous, 1000);
                        device.Filter = _filter;
                        device.StartCapture();
                        _device = device;
                        _isCapturing = true;
                        _monitor.LogCaptureStarted(device.Description, _filter);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _eventLogger.LogCaptureError($"Erro ao iniciar captura no dispositivo {device.Description}", "StartCapture", ex);
                        device.Close();
                    }
                }
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

                if (packet is UdpPacket udpPacket)
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
            }
            catch (Exception ex)
            {
                _monitor.LogCaptureError(ex, "Device_OnPacketArrival");
                _eventLogger.LogCaptureError("Erro ao processar pacote capturado", "Device_OnPacketArrival", ex);
            }
        }

        public void Stop()
        {
            if (!_isCapturing || _device == null)
            {
                return;
            }

            try
            {
                _device.StopCapture();
                _device.Close();
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