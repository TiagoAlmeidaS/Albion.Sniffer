using System;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Queue.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Queue.Publishers
{
    /// <summary>
    /// Faz a ponte entre o EventDispatcher (Core) e o IQueuePublisher (Queue)
    /// e publica cada evento recebido em um tópico padronizado.
    /// </summary>
    public sealed class EventToQueueBridge
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly IQueuePublisher _publisher;
        private readonly ILogger<EventToQueueBridge> _logger;

        public EventToQueueBridge(EventDispatcher eventDispatcher, IQueuePublisher publisher, ILogger<EventToQueueBridge> logger)
        {
            _eventDispatcher = eventDispatcher;
            _publisher = publisher;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EventToQueueBridge>.Instance;

            _eventDispatcher.RegisterGlobalHandler(OnEventAsync);
        }

        private async Task OnEventAsync(object gameEvent)
        {
            try
            {
                var eventType = gameEvent.GetType().Name;
                var timestamp = DateTime.UtcNow;

                _logger.LogInformation("🎯 EVENTO RECEBIDO: {EventType} em {Timestamp}", eventType, timestamp);

                // Mapeamento hierárquico para eventos V1
                var topic = GetHierarchicalTopic(eventType);
                if (string.IsNullOrEmpty(topic))
                {
                    // ✅ ESPECIAL: PACOTES DE DESCOBERTA (DiscoveryDebugHandler)
                    if (eventType == "DecryptedPacketData")
                    {
                        topic = "albion.discovery.raw";
                        _logger.LogInformation("🔍 DESCOBERTA: Pacote interceptado será enviado para fila de descoberta");
                    }
                    else
                    {
                        // Fallback para eventos não mapeados
                        var eventTypeFormatted = eventType.EndsWith("Event", StringComparison.Ordinal)
                            ? eventType.Substring(0, eventType.Length - "Event".Length)
                            : eventType;
                        
                        // Remove sufixo "V1" dos nomes das filas para eventos V1
                        if (eventTypeFormatted.EndsWith("V1", StringComparison.Ordinal))
                        {
                            eventTypeFormatted = eventTypeFormatted.Substring(0, eventTypeFormatted.Length - "V1".Length);
                        }
                        
                        topic = $"albion.event.{eventTypeFormatted.ToLowerInvariant()}";
                    }
                }

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
                        if (posProp != null && posProp.PropertyType == typeof(Vector2))
                        {
                            var pos = (Vector2)posProp.GetValue(gameEvent);
                            location = new { X = pos.X, Y = pos.Y };
                        }
                    }
                }
                catch { }

                // Validar e limpar o objeto antes da serialização JSON
                var cleanMessage = CleanMessageForJson(new
                {
                    EventType = eventType,
                    Timestamp = timestamp,
                    Position = location,
                    Data = gameEvent
                });

                _logger.LogInformation("📤 PUBLICANDO: {EventType} -> {Topic}", eventType, topic);
                await _publisher.PublishAsync(topic, cleanMessage);
                _logger.LogInformation("✅ Evento publicado na fila: {EventType} -> {Topic}", eventType, topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao publicar evento na fila: {EventType} - {Message}", gameEvent.GetType().Name, ex.Message);
            }
        }

        /// <summary>
        /// Mapeia tipos de eventos V1 para tópicos hierárquicos organizados por contexto
        /// </summary>
        /// <param name="eventType">Nome do tipo do evento (ex: "PlayerMovedV1")</param>
        /// <returns>Tópico hierárquico ou string vazia se não mapeado</returns>
        private static string GetHierarchicalTopic(string eventType)
        {
            return eventType switch
            {
                // Player Events 🧙‍♂️
                "PlayerSpottedV1" => "albion.event.player.spotted",
                "PlayerMovedV1" => "albion.event.player.moved",
                "PlayerJoinedV1" => "albion.event.player.joined",
                "EntityLeftV1" => "albion.event.player.left",
                "PlayerMoveRequestV1" => "albion.event.player.move.request",
                "EquipmentChangedV1" => "albion.event.player.equipment.changed",
                "MountedStateChangedV1" => "albion.event.player.mounted.changed",
                "FlaggingFinishedV1" => "albion.event.player.flagging.finished",
                "HealthUpdatedV1" => "albion.event.player.health.updated",
                "RegenerationChangedV1" => "albion.event.player.regeneration.changed",

                // Cluster Events 🗺️
                "ClusterChangedV1" => "albion.event.cluster.changed",
                "ClusterObjectsLoadedV1" => "albion.event.cluster.objects.loaded",
                "KeySyncV1" => "albion.event.cluster.key.sync",

                // Mob Events 👹
                "MobSpawnedV1" => "albion.event.mob.spawned",
                "MobStateChangedV1" => "albion.event.mob.state.changed",

                // Harvestable Events 🌿
                "HarvestableFoundV1" => "albion.event.harvestable.found",
                "HarvestablesListFoundV1" => "albion.event.harvestable.list.found",
                "HarvestableStateChangedV1" => "albion.event.harvestable.state.changed",

                // World Objects Events 🏰
                "DungeonFoundV1" => "albion.event.world.dungeon.found",
                "FishingZoneFoundV1" => "albion.event.world.fishing.zone.found",
                "LootChestFoundV1" => "albion.event.world.loot.chest.found",
                "WispGateOpenedV1" => "albion.event.world.wisp.gate.opened",
                "GatedWispFoundV1" => "albion.event.world.gated.wisp.found",

                // Mists Events 🌫️
                "MistsPlayerJoinedV1" => "albion.event.mists.player.joined",

                // Evento não mapeado
                _ => string.Empty
            };
        }

        /// <summary>
        /// Limpa e valida mensagem para serialização JSON segura
        /// </summary>
        /// <param name="message">Mensagem a ser limpa</param>
        /// <returns>Mensagem limpa e segura para JSON</returns>
        private static object CleanMessageForJson(object message)
        {
            try
            {
                // Serializar e deserializar para limpar valores problemáticos
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(message, jsonOptions);
                return JsonSerializer.Deserialize<object>(json, jsonOptions) ?? message;
            }
            catch
            {
                // Se falhar, retorna a mensagem original
                return message;
            }
        }
    }
}


