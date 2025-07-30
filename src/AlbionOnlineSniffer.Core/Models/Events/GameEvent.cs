using System;
using System.Numerics;
using Player = AlbionOnlineSniffer.Core.Models.GameObjects.Player;
using Mob = AlbionOnlineSniffer.Core.Models.GameObjects.Mob;
using Harvestable = AlbionOnlineSniffer.Core.Models.GameObjects.Harvestable;
using LootChest = AlbionOnlineSniffer.Core.Models.GameObjects.LootChest;
using Dungeon = AlbionOnlineSniffer.Core.Models.GameObjects.Dungeon;
using Cluster = AlbionOnlineSniffer.Core.Models.GameObjects.Cluster;

namespace AlbionOnlineSniffer.Core.Models.Events
{
    /// <summary>
    /// Evento base para todos os eventos do jogo
    /// </summary>
    public abstract class GameEvent
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Evento de novo jogador detectado
    /// </summary>
    public class PlayerDetectedEvent : GameEvent
    {
        public PlayerDetectedEvent(Player player)
        {
            EventType = "PlayerDetected";
            Player = player;
        }

        public Player Player { get; set; }
    }

    /// <summary>
    /// Evento de movimento de jogador
    /// </summary>
    public class PlayerMovedEvent : GameEvent
    {
        public PlayerMovedEvent(int playerId, Vector2 newPosition)
        {
            EventType = "PlayerMoved";
            PlayerId = playerId;
            NewPosition = newPosition;
        }

        public int PlayerId { get; set; }
        public Vector2 NewPosition { get; set; }
    }

    /// <summary>
    /// Evento de jogador saiu
    /// </summary>
    public class PlayerLeftEvent : GameEvent
    {
        public PlayerLeftEvent(int playerId)
        {
            EventType = "PlayerLeft";
            PlayerId = playerId;
        }

        public int PlayerId { get; set; }
    }

    /// <summary>
    /// Evento de novo mob detectado
    /// </summary>
    public class MobDetectedEvent : GameEvent
    {
        public MobDetectedEvent(Mob mob)
        {
            EventType = "MobDetected";
            Mob = mob;
        }

        public Mob Mob { get; set; }
    }

    /// <summary>
    /// Evento de mob removido
    /// </summary>
    public class MobRemovedEvent : GameEvent
    {
        public MobRemovedEvent(int mobId, Vector2? lastPosition = null)
        {
            EventType = "MobRemoved";
            MobId = mobId;
            LastPosition = lastPosition;
        }

        public int MobId { get; set; }
        public Vector2? LastPosition { get; set; }
    }

    /// <summary>
    /// Evento de novo harvestable detectado
    /// </summary>
    public class HarvestableDetectedEvent : GameEvent
    {
        public HarvestableDetectedEvent(Harvestable harvestable)
        {
            EventType = "HarvestableDetected";
            Harvestable = harvestable;
        }

        public Harvestable Harvestable { get; set; }
    }

    /// <summary>
    /// Evento de harvestable atualizado
    /// </summary>
    public class HarvestableUpdatedEvent : GameEvent
    {
        public HarvestableUpdatedEvent(int harvestableId, int count, int charge, Vector2 position)
        {
            EventType = "HarvestableUpdated";
            HarvestableId = harvestableId;
            Count = count;
            Charge = charge;
            Position = position;
        }

        public int HarvestableId { get; set; }
        public int Count { get; set; }
        public int Charge { get; set; }
        public Vector2 Position { get; set; }
    }

    /// <summary>
    /// Evento de novo loot chest detectado
    /// </summary>
    public class LootChestDetectedEvent : GameEvent
    {
        public LootChestDetectedEvent(LootChest lootChest)
        {
            EventType = "LootChestDetected";
            LootChest = lootChest;
        }

        public LootChest LootChest { get; set; }
    }

    /// <summary>
    /// Evento de nova dungeon detectada
    /// </summary>
    public class DungeonDetectedEvent : GameEvent
    {
        public DungeonDetectedEvent(Dungeon dungeon)
        {
            EventType = "DungeonDetected";
            Dungeon = dungeon;
        }

        public Dungeon Dungeon { get; set; }
    }

    /// <summary>
    /// Evento de mudança de cluster
    /// </summary>
    public class ClusterChangedEvent : GameEvent
    {
        public ClusterChangedEvent(Cluster newCluster)
        {
            EventType = "ClusterChanged";
            NewCluster = newCluster;
        }

        public Cluster NewCluster { get; set; }
    }

    /// <summary>
    /// Evento de movimento de mob
    /// </summary>
    public class MobMovedEvent : GameEvent
    {
        public MobMovedEvent(int mobId, Vector2 newPosition)
        {
            EventType = "MobMoved";
            MobId = mobId;
            NewPosition = newPosition;
        }

        public int MobId { get; set; }
        public Vector2 NewPosition { get; set; }
    }

    /// <summary>
    /// Evento de novo fish node detectado
    /// </summary>
    public class FishNodeDetectedEvent : GameEvent
    {
        public FishNodeDetectedEvent(int fishNodeId, Vector2 position, int size, int respawnCount)
        {
            EventType = "FishNodeDetected";
            FishNodeId = fishNodeId;
            Position = position;
            Size = size;
            RespawnCount = respawnCount;
        }

        public int FishNodeId { get; set; }
        public Vector2 Position { get; set; }
        public int Size { get; set; }
        public int RespawnCount { get; set; }
    }

    /// <summary>
    /// Evento de novo gated wisp detectado
    /// </summary>
    public class GatedWispDetectedEvent : GameEvent
    {
        public GatedWispDetectedEvent(int wispId, Vector2 position)
        {
            EventType = "GatedWispDetected";
            WispId = wispId;
            Position = position;
        }

        public int WispId { get; set; }
        public Vector2 Position { get; set; }
    }

    /// <summary>
    /// Evento de gated wisp coletado
    /// </summary>
    public class GatedWispCollectedEvent : GameEvent
    {
        public GatedWispCollectedEvent(int wispId, Vector2 position)
        {
            EventType = "GatedWispCollected";
            WispId = wispId;
            Position = position;
        }

        public int WispId { get; set; }
        public Vector2 Position { get; set; }
    }
} 