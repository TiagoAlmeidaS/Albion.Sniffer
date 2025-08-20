using System;
using System.Text.Json;
using MessagePack;
using Xunit;
using Albion.Events.V1;

namespace AlbionOnlineSniffer.Tests.Events;

public class V1ContractsTests
{
    [Fact]
    public void PlayerSpottedV1_SerializesCorrectly_WithMessagePack()
    {
        // Arrange
        var contract = new PlayerSpottedV1
        {
            EventId = Guid.NewGuid().ToString(),
            ObservedAt = DateTimeOffset.UtcNow,
            Cluster = "Caerleon",
            Region = "Royal",
            PlayerId = 12345,
            PlayerName = "TestPlayer",
            GuildName = "TestGuild",
            AllianceName = "TestAlliance",
            X = 100.5f,
            Y = 200.5f,
            Tier = 8
        };

        // Act
        var bytes = MessagePackSerializer.Serialize(contract);
        var deserialized = MessagePackSerializer.Deserialize<PlayerSpottedV1>(bytes);

        // Assert
        Assert.Equal(contract.EventId, deserialized.EventId);
        Assert.Equal(contract.ObservedAt, deserialized.ObservedAt);
        Assert.Equal(contract.Cluster, deserialized.Cluster);
        Assert.Equal(contract.Region, deserialized.Region);
        Assert.Equal(contract.PlayerId, deserialized.PlayerId);
        Assert.Equal(contract.PlayerName, deserialized.PlayerName);
        Assert.Equal(contract.GuildName, deserialized.GuildName);
        Assert.Equal(contract.AllianceName, deserialized.AllianceName);
        Assert.Equal(contract.X, deserialized.X);
        Assert.Equal(contract.Y, deserialized.Y);
        Assert.Equal(contract.Tier, deserialized.Tier);
    }

    [Fact]
    public void PlayerSpottedV1_SerializesCorrectly_WithJson()
    {
        // Arrange
        var contract = new PlayerSpottedV1
        {
            EventId = Guid.NewGuid().ToString(),
            ObservedAt = DateTimeOffset.UtcNow,
            Cluster = "Bridgewatch",
            Region = "Royal",
            PlayerId = 67890,
            PlayerName = "JsonPlayer",
            GuildName = null,
            AllianceName = null,
            X = 300.5f,
            Y = 400.5f,
            Tier = 6
        };

        // Act
        var json = JsonSerializer.Serialize(contract);
        var deserialized = JsonSerializer.Deserialize<PlayerSpottedV1>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(contract.EventId, deserialized.EventId);
        Assert.Equal(contract.PlayerName, deserialized.PlayerName);
        Assert.Null(deserialized.GuildName);
        Assert.Null(deserialized.AllianceName);
    }

    [Fact]
    public void LootChestFoundV1_SerializesCorrectly()
    {
        // Arrange
        var contract = new LootChestFoundV1
        {
            EventId = Guid.NewGuid().ToString(),
            ObservedAt = DateTimeOffset.UtcNow,
            Id = 99999,
            X = 1000.0f,
            Y = 2000.0f
        };

        // Act
        var bytes = MessagePackSerializer.Serialize(contract);
        var deserialized = MessagePackSerializer.Deserialize<LootChestFoundV1>(bytes);

        // Assert
        Assert.Equal(contract.EventId, deserialized.EventId);
        Assert.Equal(contract.ObservedAt, deserialized.ObservedAt);
        Assert.Equal(contract.Id, deserialized.Id);
        Assert.Equal(contract.X, deserialized.X);
        Assert.Equal(contract.Y, deserialized.Y);
    }

    [Fact]
    public void HeartbeatV1_SerializesCorrectly()
    {
        // Arrange
        var contract = new HeartbeatV1
        {
            EventId = Guid.NewGuid().ToString(),
            ObservedAt = DateTimeOffset.UtcNow,
            SnifferId = "sniffer-001",
            Version = "1.0.0",
            Uptime = 3600,
            EventsProcessed = 150000,
            PacketsCaptured = 500000
        };

        // Act
        var bytes = MessagePackSerializer.Serialize(contract);
        var deserialized = MessagePackSerializer.Deserialize<HeartbeatV1>(bytes);

        // Assert
        Assert.Equal(contract.EventId, deserialized.EventId);
        Assert.Equal(contract.SnifferId, deserialized.SnifferId);
        Assert.Equal(contract.Version, deserialized.Version);
        Assert.Equal(contract.Uptime, deserialized.Uptime);
        Assert.Equal(contract.EventsProcessed, deserialized.EventsProcessed);
        Assert.Equal(contract.PacketsCaptured, deserialized.PacketsCaptured);
    }

    [Fact]
    public void PublishFailureV1_SerializesCorrectly()
    {
        // Arrange
        var contract = new PublishFailureV1
        {
            EventId = Guid.NewGuid().ToString(),
            ObservedAt = DateTimeOffset.UtcNow,
            OriginalEventType = "PlayerSpottedV1",
            OriginalEventId = Guid.NewGuid().ToString(),
            Error = "Connection timeout",
            RetryCount = 3
        };

        // Act
        var bytes = MessagePackSerializer.Serialize(contract);
        var deserialized = MessagePackSerializer.Deserialize<PublishFailureV1>(bytes);

        // Assert
        Assert.Equal(contract.EventId, deserialized.EventId);
        Assert.Equal(contract.OriginalEventType, deserialized.OriginalEventType);
        Assert.Equal(contract.OriginalEventId, deserialized.OriginalEventId);
        Assert.Equal(contract.Error, deserialized.Error);
        Assert.Equal(contract.RetryCount, deserialized.RetryCount);
    }

    [Fact]
    public void HideoutSpottedV1_SerializesCorrectly()
    {
        // Arrange
        var contract = new HideoutSpottedV1
        {
            EventId = Guid.NewGuid().ToString(),
            ObservedAt = DateTimeOffset.UtcNow,
            HideoutId = 12345,
            GuildName = "ARCH",
            AllianceName = "ARCH Alliance",
            X = 2048.0f,
            Y = 1536.0f,
            Tier = 8
        };

        // Act
        var bytes = MessagePackSerializer.Serialize(contract);
        var deserialized = MessagePackSerializer.Deserialize<HideoutSpottedV1>(bytes);

        // Assert
        Assert.Equal(contract.EventId, deserialized.EventId);
        Assert.Equal(contract.HideoutId, deserialized.HideoutId);
        Assert.Equal(contract.GuildName, deserialized.GuildName);
        Assert.Equal(contract.AllianceName, deserialized.AllianceName);
        Assert.Equal(contract.X, deserialized.X);
        Assert.Equal(contract.Y, deserialized.Y);
        Assert.Equal(contract.Tier, deserialized.Tier);
    }

    [Fact]
    public void ClusterChangedV1_SerializesCorrectly()
    {
        // Arrange
        var contract = new ClusterChangedV1
        {
            EventId = Guid.NewGuid().ToString(),
            ObservedAt = DateTimeOffset.UtcNow,
            ClusterIndex = "1004",
            ClusterName = "Bridgewatch",
            Region = "Royal",
            MapType = "OPEN_WORLD"
        };

        // Act
        var bytes = MessagePackSerializer.Serialize(contract);
        var deserialized = MessagePackSerializer.Deserialize<ClusterChangedV1>(bytes);

        // Assert
        Assert.Equal(contract.EventId, deserialized.EventId);
        Assert.Equal(contract.ClusterIndex, deserialized.ClusterIndex);
        Assert.Equal(contract.ClusterName, deserialized.ClusterName);
        Assert.Equal(contract.Region, deserialized.Region);
        Assert.Equal(contract.MapType, deserialized.MapType);
    }

    [Fact]
    public void AllV1Contracts_HaveRequiredAttributes()
    {
        // This test ensures all V1 contracts have MessagePackObject attribute
        var contractTypes = new[]
        {
            typeof(PlayerSpottedV1),
            typeof(LootChestFoundV1),
            typeof(ClusterChangedV1),
            typeof(DungeonFoundV1),
            typeof(EntityLeftV1),
            typeof(EquipmentChangedV1),
            typeof(FishingZoneFoundV1),
            typeof(FlaggingFinishedV1),
            typeof(GatedWispFoundV1),
            typeof(HarvestableFoundV1),
            typeof(HarvestableStateChangedV1),
            typeof(HarvestablesListFoundV1),
            typeof(HealthUpdatedV1),
            typeof(HeartbeatV1),
            typeof(HideoutSpottedV1),
            typeof(KeySyncV1),
            typeof(MistsPlayerJoinedV1),
            typeof(MobSpawnedV1),
            typeof(MobStateChangedV1),
            typeof(MountedStateChangedV1),
            typeof(PublishFailureV1),
            typeof(RegenerationChangedV1),
            typeof(WispGateOpenedV1)
        };

        foreach (var type in contractTypes)
        {
            var attribute = Attribute.GetCustomAttribute(type, typeof(MessagePackObjectAttribute));
            Assert.NotNull(attribute);
            
            // Verify all have EventId and ObservedAt properties
            var eventIdProp = type.GetProperty("EventId");
            var observedAtProp = type.GetProperty("ObservedAt");
            
            Assert.NotNull(eventIdProp);
            Assert.NotNull(observedAtProp);
        }
    }
}