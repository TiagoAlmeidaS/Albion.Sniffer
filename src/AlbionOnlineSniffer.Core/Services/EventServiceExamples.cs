using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Models.Events;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Exemplos de como usar o sistema de eventos para diferentes tipos de serviços
    /// </summary>
    public static class EventServiceExamples
    {
        /// <summary>
        /// Exemplo de configuração do sistema de eventos
        /// </summary>
        public static void ConfigureEventHandlers(EventDispatcher eventDispatcher, ILogger logger)
        {
            // 1. Handler para todos os eventos (logging)
            eventDispatcher.RegisterGlobalHandler(async gameEvent =>
            {
                logger.LogInformation("Evento global: {EventType} em {Timestamp}", 
                    gameEvent.EventType, gameEvent.Timestamp);
                await Task.CompletedTask;
            });

            // 2. Handler específico para jogadores (alertas)
            eventDispatcher.RegisterHandler("PlayerDetected", async gameEvent =>
            {
                if (gameEvent is PlayerDetectedEvent playerEvent)
                {
                    logger.LogWarning("🚨 JOGADOR DETECTADO: {Name} (Guild: {Guild})", 
                        playerEvent.Player.Name, playerEvent.Player.Guild);
                    
                    // Aqui você pode enviar para diferentes serviços:
                    // - Discord webhook
                    // - Telegram bot
                    // - Email
                    // - SMS
                    // - Sistema de alertas
                }
                await Task.CompletedTask;
            });

            // 3. Handler para mobs (notificações)
            eventDispatcher.RegisterHandler("MobDetected", async gameEvent =>
            {
                if (gameEvent is MobDetectedEvent mobEvent)
                {
                    logger.LogInformation("🐉 MOB DETECTADO: {MobName} (Tipo: {Type})", 
                        mobEvent.Mob.MobInfo.MobName, mobEvent.Mob.MobInfo.Type);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de mobs
                    // - Notificações de boss
                    // - Analytics
                }
                await Task.CompletedTask;
            });

            // 4. Handler para harvestables (recursos)
            eventDispatcher.RegisterHandler("HarvestableDetected", async gameEvent =>
            {
                if (gameEvent is HarvestableDetectedEvent harvestableEvent)
                {
                    logger.LogInformation("🌿 RECURSO DETECTADO: {Type} T{Level} (Count: {Count})", 
                        harvestableEvent.Harvestable.Type, 
                        harvestableEvent.Harvestable.Tier, 
                        harvestableEvent.Harvestable.Count);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de farming
                    // - Mapeamento de recursos
                    // - Alertas de recursos raros
                }
                await Task.CompletedTask;
            });

            // 5. Handler para loot chests (baús)
            eventDispatcher.RegisterHandler("LootChestDetected", async gameEvent =>
            {
                if (gameEvent is LootChestDetectedEvent chestEvent)
                {
                    logger.LogInformation("📦 BAÚ DETECTADO: {Name} (Charge: {Charge})", 
                        chestEvent.LootChest.Name, chestEvent.LootChest.Charge);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de baús
                    // - Alertas de baús raros
                    // - Mapeamento de loot
                }
                await Task.CompletedTask;
            });

            // 6. Handler para dungeons
            eventDispatcher.RegisterHandler("DungeonDetected", async gameEvent =>
            {
                if (gameEvent is DungeonDetectedEvent dungeonEvent)
                {
                    logger.LogInformation("🏰 DUNGEON DETECTADA: {Type} (Charges: {Charges})", 
                        dungeonEvent.Dungeon.Type, dungeonEvent.Dungeon.Charges);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de dungeons
                    // - Alertas de dungeons corrompidas
                    // - Mapeamento de dungeons
                }
                await Task.CompletedTask;
            });

            // 7. Handler para mudanças de cluster
            eventDispatcher.RegisterHandler("ClusterChanged", async gameEvent =>
            {
                if (gameEvent is ClusterChangedEvent clusterEvent)
                {
                    logger.LogInformation("🗺️ CLUSTER MUDOU: {DisplayName} ({Color})", 
                        clusterEvent.NewCluster.DisplayName, clusterEvent.NewCluster.ClusterColor);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de localização
                    // - Alertas de mudança de zona
                    // - Analytics de movimento
                }
                await Task.CompletedTask;
            });

            // 8. Handler para movimentos de jogadores
            eventDispatcher.RegisterHandler("PlayerMoved", async gameEvent =>
            {
                if (gameEvent is PlayerMovedEvent moveEvent)
                {
                    logger.LogDebug("👤 JOGADOR MOVEU: ID {PlayerId} -> ({X}, {Y})", 
                        moveEvent.PlayerId, moveEvent.NewPosition.X, moveEvent.NewPosition.Y);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de movimento
                    // - Analytics de comportamento
                    // - Sistema de radar
                }
                await Task.CompletedTask;
            });

            // 9. Handler para jogadores que saíram
            eventDispatcher.RegisterHandler("PlayerLeft", async gameEvent =>
            {
                if (gameEvent is PlayerLeftEvent leaveEvent)
                {
                    logger.LogInformation("👋 JOGADOR SAIU: ID {PlayerId}", leaveEvent.PlayerId);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de limpeza de dados
                    // - Analytics de saída
                    // - Notificações de saída
                }
                await Task.CompletedTask;
            });

            // 10. Handler para mobs removidos
            eventDispatcher.RegisterHandler("MobRemoved", async gameEvent =>
            {
                if (gameEvent is MobRemovedEvent mobEvent)
                {
                    if (mobEvent.LastPosition.HasValue)
                    {
                        logger.LogInformation("💀 MOB REMOVIDO: ID {MobId} em ({X}, {Y})", 
                            mobEvent.MobId, mobEvent.LastPosition.Value.X, mobEvent.LastPosition.Value.Y);
                    }
                    else
                    {
                        logger.LogInformation("💀 MOB REMOVIDO: ID {MobId}", mobEvent.MobId);
                    }
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de morte
                    // - Analytics de combate
                    // - Notificações de morte
                }
                await Task.CompletedTask;
            });

            // 11. Handler para mobs movidos
            eventDispatcher.RegisterHandler("MobMoved", async gameEvent =>
            {
                if (gameEvent is MobMovedEvent mobEvent)
                {
                    logger.LogDebug("🐉 MOB MOVEU: ID {MobId} -> ({X}, {Y})", 
                        mobEvent.MobId, mobEvent.NewPosition.X, mobEvent.NewPosition.Y);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de movimento
                    // - Analytics de comportamento
                    // - Sistema de radar
                }
                await Task.CompletedTask;
            });

            // 12. Handler para harvestables atualizados
            eventDispatcher.RegisterHandler("HarvestableUpdated", async gameEvent =>
            {
                if (gameEvent is HarvestableUpdatedEvent updateEvent)
                {
                    logger.LogDebug("🔄 HARVESTABLE ATUALIZADO: ID {Id} (Count: {Count}, Charge: {Charge}) em ({X}, {Y})", 
                        updateEvent.HarvestableId, updateEvent.Count, updateEvent.Charge, 
                        updateEvent.Position.X, updateEvent.Position.Y);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de coleta
                    // - Analytics de farming
                    // - Notificações de coleta
                }
                await Task.CompletedTask;
            });

            // 13. Handler para fish nodes detectados
            eventDispatcher.RegisterHandler("FishNodeDetected", async gameEvent =>
            {
                if (gameEvent is FishNodeDetectedEvent fishEvent)
                {
                    logger.LogInformation("🐟 FISH NODE DETECTADO: ID {Id} (Size: {Size}) em ({X}, {Y})", 
                        fishEvent.FishNodeId, fishEvent.Size, fishEvent.Position.X, fishEvent.Position.Y);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de pesca
                    // - Mapeamento de zonas de pesca
                    // - Alertas de pesca
                }
                await Task.CompletedTask;
            });

            // 14. Handler para gated wisps detectados
            eventDispatcher.RegisterHandler("GatedWispDetected", async gameEvent =>
            {
                if (gameEvent is GatedWispDetectedEvent wispEvent)
                {
                    logger.LogInformation("✨ GATED WISP DETECTADO: ID {Id} em ({X}, {Y})", 
                        wispEvent.WispId, wispEvent.Position.X, wispEvent.Position.Y);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de wisps
                    // - Mapeamento de wisps
                    // - Alertas de wisps
                }
                await Task.CompletedTask;
            });

            // 15. Handler para gated wisps coletados
            eventDispatcher.RegisterHandler("GatedWispCollected", async gameEvent =>
            {
                if (gameEvent is GatedWispCollectedEvent wispEvent)
                {
                    logger.LogInformation("🎉 GATED WISP COLETADO: ID {Id} em ({X}, {Y})", 
                        wispEvent.WispId, wispEvent.Position.X, wispEvent.Position.Y);
                    
                    // Aqui você pode enviar para:
                    // - Sistema de tracking de coleta
                    // - Analytics de wisps
                    // - Notificações de coleta
                }
                await Task.CompletedTask;
            });

            // 16. Handler para sincronização de chave XOR
            eventDispatcher.RegisterHandler("KeySync", async gameEvent =>
            {
                if (gameEvent is KeySyncEvent keyEvent)
                {
                    logger.LogInformation("🔑 CHAVE XOR SINCRONIZADA: {KeyLength} bytes", keyEvent.Code?.Length ?? 0);
                    // Aqui você pode integrar com serviços de decriptação
                    // await decryptionService.ProcessKeySync(keyEvent);
                }
                await Task.CompletedTask;
            });

            // 17. Handler para regeneração de vida
            eventDispatcher.RegisterHandler("RegenerationChanged", async gameEvent =>
            {
                if (gameEvent is RegenerationChangedEvent regenEvent)
                {
                    logger.LogInformation("💚 REGENERAÇÃO ALTERADA: ID {Id}, Vida: {Current}/{Max}", 
                        regenEvent.Id, regenEvent.Health.Value, regenEvent.Health.MaxValue);
                    // Aqui você pode integrar com serviços de monitoramento de vida
                    // await healthService.ProcessRegeneration(regenEvent);
                }
                await Task.CompletedTask;
            });

            // 18. Handler para informação de mists
            eventDispatcher.RegisterHandler("MistsPlayerJoinedInfo", async gameEvent =>
            {
                if (gameEvent is MistsPlayerJoinedInfoEvent mistsEvent)
                {
                    logger.LogInformation("🌫️ MISTS INFO: TimeCycle = {TimeCycle}", 
                        mistsEvent.TimeCycle.ToString("yyyy-MM-dd HH:mm:ss"));
                    // Aqui você pode integrar com serviços de mists
                    // await mistsService.ProcessPlayerJoined(mistsEvent);
                }
                await Task.CompletedTask;
            });

            // 19. Handler para objetos de cluster
            eventDispatcher.RegisterHandler("LoadClusterObjects", async gameEvent =>
            {
                if (gameEvent is LoadClusterObjectsEvent clusterEvent)
                {
                    logger.LogInformation("🗺️ CLUSTER OBJECTS: {Count} objetivos", 
                        clusterEvent.ClusterObjectives?.Count ?? 0);
                    // Aqui você pode integrar com serviços de cluster
                    // await clusterService.ProcessClusterObjects(clusterEvent);
                }
                await Task.CompletedTask;
            });

            // 20. Handler para montaria
            eventDispatcher.RegisterHandler("Mounted", async gameEvent =>
            {
                if (gameEvent is MountedEvent mountedEvent)
                {
                    var status = mountedEvent.IsMounted ? "montado" : "desmontado";
                    logger.LogInformation("🐎 MONTARIA: Player {Id} {Status}", mountedEvent.Id, status);
                    // Aqui você pode integrar com serviços de montaria
                    // await mountService.ProcessMounted(mountedEvent);
                }
                await Task.CompletedTask;
            });

            // 21. Handler para mudança de equipamento
            eventDispatcher.RegisterHandler("CharacterEquipmentChanged", async gameEvent =>
            {
                if (gameEvent is CharacterEquipmentChangedEvent equipmentEvent)
                {
                    logger.LogInformation("⚔️ EQUIPAMENTO: Player {Id}, {EquipmentCount} equipamentos, {SpellCount} spells", 
                        equipmentEvent.Id, equipmentEvent.Equipments?.Count ?? 0, equipmentEvent.Spells?.Count ?? 0);
                    // Aqui você pode integrar com serviços de equipamento
                    // await equipmentService.ProcessEquipmentChanged(equipmentEvent);
                }
                await Task.CompletedTask;
            });

            // 22. Handler para mudança de flagging finalizada
            eventDispatcher.RegisterHandler("ChangeFlaggingFinished", async gameEvent =>
            {
                if (gameEvent is ChangeFlaggingFinishedEvent flaggingEvent)
                {
                    logger.LogInformation("🏁 FLAGGING FINALIZADA: Player {Id}, Faction: {Faction}", 
                        flaggingEvent.Id, flaggingEvent.Faction);
                    // Aqui você pode integrar com serviços de flagging
                    // await flaggingService.ProcessFlaggingFinished(flaggingEvent);
                }
                await Task.CompletedTask;
            });

            // 23. Handler para wisp gate aberto
            eventDispatcher.RegisterHandler("WispGateOpened", async gameEvent =>
            {
                if (gameEvent is WispGateOpenedEvent wispGateEvent)
                {
                    var status = wispGateEvent.IsCollected ? "coletado" : "aberto";
                    logger.LogInformation("✨ WISP GATE {Status}: ID {Id}", status, wispGateEvent.Id);
                    // Aqui você pode integrar com serviços de wisp
                    // await wispService.ProcessWispGateOpened(wispGateEvent);
                }
                await Task.CompletedTask;
            });

            // 24. Handler para nova zona de pesca
            eventDispatcher.RegisterHandler("NewFishingZone", async gameEvent =>
            {
                if (gameEvent is NewFishingZoneEvent fishingZoneEvent)
                {
                    logger.LogInformation("🐟 NOVA ZONA DE PESCA: ID {Id} (Size: {Size}, Respawn: {Respawn}) em ({X}, {Y})", 
                        fishingZoneEvent.Id, fishingZoneEvent.Size, fishingZoneEvent.RespawnCount, 
                        fishingZoneEvent.Position.X, fishingZoneEvent.Position.Y);
                    // Aqui você pode integrar com serviços de pesca
                    // await fishingService.ProcessNewFishingZone(fishingZoneEvent);
                }
                await Task.CompletedTask;
            });

            // 25. Handler para nova saída de dungeon
            eventDispatcher.RegisterHandler("NewDungeonExit", async gameEvent =>
            {
                if (gameEvent is NewDungeonExitEvent dungeonExitEvent)
                {
                    logger.LogInformation("🏰 NOVA SAÍDA DE DUNGEON: ID {Id} ({Type}, Charges: {Charges}) em ({X}, {Y})", 
                        dungeonExitEvent.Id, dungeonExitEvent.Type, dungeonExitEvent.Charges, 
                        dungeonExitEvent.Position.X, dungeonExitEvent.Position.Y);
                    // Aqui você pode integrar com serviços de dungeon
                    // await dungeonService.ProcessNewDungeonExit(dungeonExitEvent);
                }
                await Task.CompletedTask;
            });

            // 26. Handler para mudança de estado de harvestable
            eventDispatcher.RegisterHandler("HarvestableChangeState", async gameEvent =>
            {
                if (gameEvent is HarvestableChangeStateEvent harvestableStateEvent)
                {
                    logger.LogInformation("🌿 HARVESTABLE MUDOU ESTADO: ID {Id} (Count: {Count}, Charge: {Charge}) em ({X}, {Y})", 
                        harvestableStateEvent.Id, harvestableStateEvent.Count, harvestableStateEvent.Charge, 
                        harvestableStateEvent.Position.X, harvestableStateEvent.Position.Y);
                    // Aqui você pode integrar com serviços de harvestable
                    // await harvestableService.ProcessChangeState(harvestableStateEvent);
                }
                await Task.CompletedTask;
            });

            // 27. Handler para mudança de estado de mob
            eventDispatcher.RegisterHandler("MobChangeState", async gameEvent =>
            {
                if (gameEvent is MobChangeStateEvent mobStateEvent)
                {
                    var status = mobStateEvent.IsDead ? "morto" : "vivo";
                    logger.LogInformation("🐉 MOB MUDOU ESTADO: ID {Id} ({Status}) em ({X}, {Y})", 
                        mobStateEvent.Id, status, mobStateEvent.Position.X, mobStateEvent.Position.Y);
                    // Aqui você pode integrar com serviços de mob
                    // await mobService.ProcessChangeState(mobStateEvent);
                }
                await Task.CompletedTask;
            });

            // 28. Handler para atualização de vida
            eventDispatcher.RegisterHandler("HealthUpdate", async gameEvent =>
            {
                if (gameEvent is HealthUpdateEvent healthUpdateEvent)
                {
                    logger.LogDebug("💚 ATUALIZAÇÃO DE VIDA: ID {Id}, Vida: {Current}/{Max} ({Percent}%)", 
                        healthUpdateEvent.Id, healthUpdateEvent.Health.Value, healthUpdateEvent.Health.MaxValue, 
                        healthUpdateEvent.Health.Percent);
                    // Aqui você pode integrar com serviços de vida
                    // await healthService.ProcessHealthUpdate(healthUpdateEvent);
                }
                await Task.CompletedTask;
            });

            // 29. Handler para nova lista de harvestables
            eventDispatcher.RegisterHandler("NewHarvestablesList", async gameEvent =>
            {
                if (gameEvent is NewHarvestablesListEvent harvestablesListEvent)
                {
                    logger.LogInformation("🌿 NOVA LISTA DE HARVESTABLES: {Count} recursos", 
                        harvestablesListEvent.Harvestables?.Count ?? 0);
                    // Aqui você pode integrar com serviços de harvestables
                    // await harvestableService.ProcessNewList(harvestablesListEvent);
                }
                await Task.CompletedTask;
            });

            // 30. Handler para request de movimento
            eventDispatcher.RegisterHandler("MoveRequest", async gameEvent =>
            {
                if (gameEvent is MoveRequestEvent moveRequestEvent)
                {
                    var status = moveRequestEvent.IsMoving ? "movendo" : "parado";
                    logger.LogDebug("🚶 REQUEST DE MOVIMENTO: ID {Id} ({Status}) para ({X}, {Y})", 
                        moveRequestEvent.Id, status, moveRequestEvent.Position.X, moveRequestEvent.Position.Y);
                    // Aqui você pode integrar com serviços de movimento
                    // await movementService.ProcessMoveRequest(moveRequestEvent);
                }
                await Task.CompletedTask;
            });

            // 31. Handler para resposta de join
            eventDispatcher.RegisterHandler("JoinResponse", async gameEvent =>
            {
                if (gameEvent is JoinResponseEvent joinResponseEvent)
                {
                    var status = joinResponseEvent.Success ? "sucesso" : "falha";
                    logger.LogInformation("🎯 RESPOSTA DE JOIN: {Status} - {Message}", 
                        status, joinResponseEvent.Message);
                    // Aqui você pode integrar com serviços de conexão
                    // await connectionService.ProcessJoinResponse(joinResponseEvent);
                }
                await Task.CompletedTask;
            });
        }

        /// <summary>
        /// Exemplo de como configurar diferentes tipos de serviços
        /// </summary>
        public static void ConfigureDifferentServices(EventDispatcher eventDispatcher, ILogger logger)
        {
            // Serviço de Alertas (Discord/Telegram)
            eventDispatcher.RegisterHandler("PlayerDetected", async gameEvent =>
            {
                // Enviar para Discord
                await SendToDiscord(gameEvent);
                
                // Enviar para Telegram
                await SendToTelegram(gameEvent);
                
                // Enviar para Email
                await SendToEmail(gameEvent);
            });

            // Serviço de Analytics
            eventDispatcher.RegisterGlobalHandler(async gameEvent =>
            {
                await SendToAnalytics(gameEvent);
            });

            // Serviço de Radar/Overlay
            eventDispatcher.RegisterHandler("PlayerDetected", async gameEvent =>
            {
                await UpdateRadar(gameEvent);
            });

            eventDispatcher.RegisterHandler("PlayerMoved", async gameEvent =>
            {
                await UpdateRadar(gameEvent);
            });

            // Serviço de Database
            eventDispatcher.RegisterGlobalHandler(async gameEvent =>
            {
                await SaveToDatabase(gameEvent);
            });

            // Serviço de Logging Avançado
            eventDispatcher.RegisterGlobalHandler(async gameEvent =>
            {
                await LogToFile(gameEvent);
            });
        }

        // Métodos de exemplo para diferentes serviços
        private static Task SendToDiscord(GameEvent gameEvent) => Task.CompletedTask;
        private static Task SendToTelegram(GameEvent gameEvent) => Task.CompletedTask;
        private static Task SendToEmail(GameEvent gameEvent) => Task.CompletedTask;
        private static Task SendToAnalytics(GameEvent gameEvent) => Task.CompletedTask;
        private static Task UpdateRadar(GameEvent gameEvent) => Task.CompletedTask;
        private static Task SaveToDatabase(GameEvent gameEvent) => Task.CompletedTask;
        private static Task LogToFile(GameEvent gameEvent) => Task.CompletedTask;
    }
} 