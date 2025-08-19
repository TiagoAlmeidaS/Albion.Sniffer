using System.Text.Json;
using Albion.Events.V1;
using AlbionOnlineSniffer.Tests.Common;
using FluentAssertions;
using MessagePack;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Core;

public class ContractsV1_SnapshotTests : IAsyncLifetime
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly MessagePackSerializerOptions _msgPackOptions;

    public ContractsV1_SnapshotTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _msgPackOptions = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.None);
    }

    public Task InitializeAsync()
    {
        return VerifyMessagePack.Initialize();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PlayerSpottedV1_Serializes_Stable()
    {
        // Arrange
        var evt = new PlayerSpottedV1(
            EventId: Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Timestamp: DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
            Name: "TestPlayer",
            Guild: "TestGuild",
            Alliance: "TestAlliance",
            Tier: 8,
            X: 100.5f,
            Y: 200.75f,
            Z: 0f,
            DistanceMeters: 1337.5
        );

        // Act - JSON
        var json = JsonSerializer.Serialize(evt, _jsonOptions);
        
        // Assert - JSON Snapshot
        await Verify(json)
            .UseMethodName($"{nameof(PlayerSpottedV1_Serializes_Stable)}_JSON")
            .UseDirectory("../Snapshots/ContractsV1");

        // Act - MessagePack
        var msgpack = MessagePackSerializer.Serialize(evt, _msgPackOptions);
        var msgpackHex = BitConverter.ToString(msgpack).Replace("-", " ");
        
        // Assert - MessagePack Snapshot
        await Verify(msgpackHex)
            .UseMethodName($"{nameof(PlayerSpottedV1_Serializes_Stable)}_MessagePack")
            .UseDirectory("../Snapshots/ContractsV1");
    }

    [Fact]
    public async Task MobSpawnedV1_Serializes_Stable()
    {
        // Arrange
        var evt = new MobSpawnedV1(
            EventId: Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"),
            Timestamp: DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
            MobType: "Wolf",
            Tier: 5,
            X: 150.25f,
            Y: 250.50f,
            Z: 10f,
            Health: 5000,
            MaxHealth: 5000
        );

        // Act - JSON
        var json = JsonSerializer.Serialize(evt, _jsonOptions);
        
        // Assert - JSON Snapshot
        await Verify(json)
            .UseMethodName($"{nameof(MobSpawnedV1_Serializes_Stable)}_JSON")
            .UseDirectory("../Snapshots/ContractsV1");

        // Act - MessagePack
        var msgpack = MessagePackSerializer.Serialize(evt, _msgPackOptions);
        var msgpackHex = BitConverter.ToString(msgpack).Replace("-", " ");
        
        // Assert - MessagePack Snapshot
        await Verify(msgpackHex)
            .UseMethodName($"{nameof(MobSpawnedV1_Serializes_Stable)}_MessagePack")
            .UseDirectory("../Snapshots/ContractsV1");
    }

    [Fact]
    public async Task HarvestableFoundV1_Serializes_Stable()
    {
        // Arrange
        var evt = new HarvestableFoundV1(
            EventId: Guid.Parse("cccccccc-dddd-eeee-ffff-000000000000"),
            Timestamp: DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
            ResourceType: "Wood",
            Tier: 6,
            X: 300.0f,
            Y: 400.0f,
            Z: 0f,
            Charges: 5
        );

        // Act - JSON
        var json = JsonSerializer.Serialize(evt, _jsonOptions);
        
        // Assert - JSON Snapshot
        await Verify(json)
            .UseMethodName($"{nameof(HarvestableFoundV1_Serializes_Stable)}_JSON")
            .UseDirectory("../Snapshots/ContractsV1");

        // Act - MessagePack
        var msgpack = MessagePackSerializer.Serialize(evt, _msgPackOptions);
        var msgpackHex = BitConverter.ToString(msgpack).Replace("-", " ");
        
        // Assert - MessagePack Snapshot
        await Verify(msgpackHex)
            .UseMethodName($"{nameof(HarvestableFoundV1_Serializes_Stable)}_MessagePack")
            .UseDirectory("../Snapshots/ContractsV1");
    }

    [Fact]
    public async Task ClusterChangedV1_Serializes_Stable()
    {
        // Arrange
        var evt = new ClusterChangedV1(
            EventId: Guid.Parse("dddddddd-eeee-ffff-0000-111111111111"),
            Timestamp: DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
            FromCluster: "Caerleon",
            ToCluster: "Bridgewatch",
            ZoneType: "Blue",
            Tier: 4
        );

        // Act - JSON
        var json = JsonSerializer.Serialize(evt, _jsonOptions);
        
        // Assert - JSON Snapshot
        await Verify(json)
            .UseMethodName($"{nameof(ClusterChangedV1_Serializes_Stable)}_JSON")
            .UseDirectory("../Snapshots/ContractsV1");

        // Act - MessagePack
        var msgpack = MessagePackSerializer.Serialize(evt, _msgPackOptions);
        var msgpackHex = BitConverter.ToString(msgpack).Replace("-", " ");
        
        // Assert - MessagePack Snapshot
        await Verify(msgpackHex)
            .UseMethodName($"{nameof(ClusterChangedV1_Serializes_Stable)}_MessagePack")
            .UseDirectory("../Snapshots/ContractsV1");
    }

    [Theory]
    [InlineData("PlayerSpottedV1")]
    [InlineData("MobSpawnedV1")]
    [InlineData("HarvestableFoundV1")]
    [InlineData("ClusterChangedV1")]
    public void Contract_Deserialization_RoundTrip_JSON(string eventType)
    {
        // Arrange
        object evt = eventType switch
        {
            "PlayerSpottedV1" => new PlayerSpottedV1(
                Guid.NewGuid(), DateTimeOffset.UtcNow, "Player", "Guild", "Alliance", 5, 1f, 2f, 3f, 100.0),
            "MobSpawnedV1" => new MobSpawnedV1(
                Guid.NewGuid(), DateTimeOffset.UtcNow, "Wolf", 3, 1f, 2f, 3f, 1000, 1000),
            "HarvestableFoundV1" => new HarvestableFoundV1(
                Guid.NewGuid(), DateTimeOffset.UtcNow, "Wood", 4, 1f, 2f, 3f, 5),
            "ClusterChangedV1" => new ClusterChangedV1(
                Guid.NewGuid(), DateTimeOffset.UtcNow, "From", "To", "Blue", 3),
            _ => throw new ArgumentException($"Unknown event type: {eventType}")
        };

        // Act
        var json = JsonSerializer.Serialize(evt, evt.GetType(), _jsonOptions);
        var deserialized = JsonSerializer.Deserialize(json, evt.GetType(), _jsonOptions);

        // Assert
        deserialized.Should().BeEquivalentTo(evt);
    }

    [Theory]
    [InlineData("PlayerSpottedV1")]
    [InlineData("MobSpawnedV1")]
    [InlineData("HarvestableFoundV1")]
    [InlineData("ClusterChangedV1")]
    public void Contract_Deserialization_RoundTrip_MessagePack(string eventType)
    {
        // Arrange
        object evt = eventType switch
        {
            "PlayerSpottedV1" => new PlayerSpottedV1(
                Guid.NewGuid(), DateTimeOffset.UtcNow, "Player", "Guild", "Alliance", 5, 1f, 2f, 3f, 100.0),
            "MobSpawnedV1" => new MobSpawnedV1(
                Guid.NewGuid(), DateTimeOffset.UtcNow, "Wolf", 3, 1f, 2f, 3f, 1000, 1000),
            "HarvestableFoundV1" => new HarvestableFoundV1(
                Guid.NewGuid(), DateTimeOffset.UtcNow, "Wood", 4, 1f, 2f, 3f, 5),
            "ClusterChangedV1" => new ClusterChangedV1(
                Guid.NewGuid(), DateTimeOffset.UtcNow, "From", "To", "Blue", 3),
            _ => throw new ArgumentException($"Unknown event type: {eventType}")
        };

        // Act
        var msgpack = MessagePackSerializer.Serialize(evt, evt.GetType(), _msgPackOptions);
        var deserialized = MessagePackSerializer.Deserialize(evt.GetType(), msgpack, _msgPackOptions);

        // Assert
        deserialized.Should().BeEquivalentTo(evt);
    }
}