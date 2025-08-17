using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Capture;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Web.Services
{
    /// <summary>
    /// Conecta a captura UDP ao desserializador e faz broadcast para a UI e métricas.
    /// </summary>
    public sealed class SnifferWebPipeline
    {
        private readonly IPacketCaptureService _capture;
        private readonly Protocol16Deserializer _deserializer;
        private readonly IHubContext<SnifferHub> _hubContext;
        private readonly EventStreamService _stream;
        private readonly InboundMessageService _inbound;
        private readonly MetricsService _metrics;
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<SnifferWebPipeline> _logger;

        public SnifferWebPipeline(
            IPacketCaptureService capture,
            Protocol16Deserializer deserializer,
            IHubContext<SnifferHub> hubContext,
            EventStreamService stream,
            InboundMessageService inbound,
            MetricsService metrics,
            EventDispatcher eventDispatcher,
            ILogger<SnifferWebPipeline> logger)
        {
            _capture = capture;
            _deserializer = deserializer;
            _hubContext = hubContext;
            _stream = stream;
            _inbound = inbound;
            _metrics = metrics;
            _eventDispatcher = eventDispatcher;
            _logger = logger;

            WireCapture();
            WireEventDispatcher();
        }

        public void Start()
        {
            _logger.LogInformation("🚀 Iniciando captura de pacotes...");
            _capture.Start();
            _logger.LogInformation("✅ Captura iniciada com sucesso! 📡 Aguardando pacotes...");
        }

        public void Stop()
        {
            _logger.LogInformation("🛑 Parando captura...");
            _capture.Stop();
            _logger.LogInformation("✅ Captura parada.");
        }

        private void WireCapture()
        {
            _capture.OnUdpPayloadCaptured += async payload =>
            {
                try
                {
                    var startTime = DateTime.UtcNow;

                    _logger.LogInformation("📡 PACOTE UDP CAPTURADO: {Length} bytes", payload?.Length ?? 0);
                    if (payload != null && payload.Length > 0)
                    {
                        _logger.LogDebug("📊 PAYLOAD HEX: {Hex}", Convert.ToHexString(payload));

                        await _inbound.HandlePacketAsync(payload, "udp-capture");

                        _stream.AddRawPacket(payload);
                        await _hubContext.Clients.All.SendAsync("udpPayload", new
                        {
                            Length = payload.Length,
                            Hex = Convert.ToHexString(payload)
                        });

                        _deserializer.ReceivePacket(payload);

                        _metrics.IncrementPacketsReceived();
                        var latency = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        _metrics.RecordPacketProcessingLatency((long)latency);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao processar pacote: {Message}", ex.Message);
                    _metrics.IncrementErrors();
                }
            };

            if (_capture is PacketCaptureService packetCapture)
            {
                packetCapture.Monitor.OnMetricsUpdated += metrics =>
                {
                    _stream.UpdateMetrics(metrics);
                    _hubContext.Clients.All.SendAsync("metrics", metrics);
                    _logger.LogDebug("📊 MÉTRICAS: {Packets} pacotes, {BytesPerSecond} B/s",
                        metrics.ValidPacketsCaptured, metrics.BytesPerSecond);
                };
            }
        }

        private void WireEventDispatcher()
        {
            _eventDispatcher.RegisterGlobalHandler(async gameEvent =>
            {
                try
                {
                    var startTime = DateTime.UtcNow;

                    object? location = null;
                    try
                    {
                        if (gameEvent is AlbionOnlineSniffer.Core.Models.Events.IHasPosition hasPosition)
                        {
                            var pos = hasPosition.Position;
                            location = new { X = pos.X, Y = pos.Y };
                        }
                        else
                        {
                            var posProp = gameEvent.GetType().GetProperty("Position");
                            if (posProp != null && posProp.PropertyType == typeof(System.Numerics.Vector2))
                            {
                                var posValue = posProp.GetValue(gameEvent);
                                if (posValue != null)
                                {
                                    var pos = (System.Numerics.Vector2)posValue;
                                    location = new { X = pos.X, Y = pos.Y };
                                }
                            }
                        }
                    }
                    catch { }

                    var eventType = gameEvent.GetType().Name;
                    var timestamp = DateTime.UtcNow;

                    _logger.LogInformation("🎯 EVENTO RECEBIDO: {EventType} em {Timestamp}", eventType, timestamp);
                    if (location != null)
                    {
                        _logger.LogInformation("📍 POSIÇÃO: {Position}", location);
                    }

                    var message = new
                    {
                        EventType = eventType,
                        Timestamp = timestamp,
                        Position = location,
                        Data = gameEvent
                    };

                    await _inbound.HandleGameEventAsync(gameEvent);

                    _stream.AddEvent(message);
                    await _hubContext.Clients.All.SendAsync("gameEvent", message);

                    _metrics.IncrementEventsProcessed();
                    var latency = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _metrics.RecordEventProcessingLatency((long)latency);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao processar evento: {Message}", ex.Message);
                    _metrics.IncrementErrors();
                }
            });
        }
    }
}


