using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MessagePack;
using Xunit;
using Albion.Events.V1;

namespace AlbionOnlineSniffer.Tests.Contract.V1
{
    public class PlayerSpottedV1ContractTests
    {
        [Fact]
        public async Task PlayerSpottedV1_ShouldSerialize_ToMessagePack()
        {
            // Arrange
            var contract = new PlayerSpottedV1
            {
                EventId = "test-event-123",
                ObservedAt = DateTimeOffset.Parse("2024-01-01T12:00:00Z"),
                Cluster = "TestCluster",
                Region = "TestRegion",
                PlayerId = 456,
                PlayerName = "TestPlayer",
                GuildName = "TestGuild",
                AllianceName = "TestAlliance",
                X = 100.5f,
                Y = 200.3f,
                Tier = 6
            };

            // Act
            var bytes = MessagePackSerializer.Serialize(contract);
            var roundTrip = MessagePackSerializer.Deserialize<PlayerSpottedV1>(bytes);

            // Assert
            roundTrip.Should().NotBeNull();
            roundTrip.EventId.Should().Be(contract.EventId);
            roundTrip.ObservedAt.Should().Be(contract.ObservedAt);
            roundTrip.Cluster.Should().Be(contract.Cluster);
            roundTrip.Region.Should().Be(contract.Region);
            roundTrip.PlayerId.Should().Be(contract.PlayerId);
            roundTrip.PlayerName.Should().Be(contract.PlayerName);
            roundTrip.GuildName.Should().Be(contract.GuildName);
            roundTrip.AllianceName.Should().Be(contract.AllianceName);
            roundTrip.X.Should().Be(contract.X);
            roundTrip.Y.Should().Be(contract.Y);
            roundTrip.Tier.Should().Be(contract.Tier);
        }

        [Fact]
        public async Task PlayerSpottedV1_ShouldSerialize_ToJson()
        {
            // Arrange
            var contract = new PlayerSpottedV1
            {
                EventId = "test-event-456",
                ObservedAt = DateTimeOffset.Parse("2024-01-01T13:00:00Z"),
                Cluster = "AnotherCluster",
                Region = "AnotherRegion",
                PlayerId = 789,
                PlayerName = "AnotherPlayer",
                GuildName = null, // Test null value
                AllianceName = null, // Test null value
                X = 150.7f,
                Y = 250.9f,
                Tier = 8
            };

            // Act
            var json = JsonSerializer.Serialize(contract);
            var roundTrip = JsonSerializer.Deserialize<PlayerSpottedV1>(json);

            // Assert
            roundTrip.Should().NotBeNull();
            roundTrip!.EventId.Should().Be(contract.EventId);
            roundTrip.ObservedAt.Should().Be(contract.ObservedAt);
            roundTrip.Cluster.Should().Be(contract.Cluster);
            roundTrip.Region.Should().Be(contract.Region);
            roundTrip.PlayerId.Should().Be(contract.PlayerId);
            roundTrip.PlayerName.Should().Be(contract.PlayerName);
            roundTrip.GuildName.Should().Be(contract.GuildName);
            roundTrip.AllianceName.Should().Be(contract.AllianceName);
            roundTrip.X.Should().Be(contract.X);
            roundTrip.Y.Should().Be(contract.Y);
            roundTrip.Tier.Should().Be(contract.Tier);
        }

        [Fact]
        public async Task PlayerSpottedV1_WithMinimalData_ShouldSerialize()
        {
            // Arrange
            var contract = new PlayerSpottedV1
            {
                EventId = "minimal-event",
                ObservedAt = DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
                Cluster = "",
                Region = "",
                PlayerId = 0,
                PlayerName = "MinimalPlayer",
                X = 0f,
                Y = 0f,
                Tier = 0
            };

            // Act
            var bytes = MessagePackSerializer.Serialize(contract);
            var roundTrip = MessagePackSerializer.Deserialize<PlayerSpottedV1>(bytes);

            // Assert
            roundTrip.Should().NotBeNull();
            roundTrip.PlayerName.Should().Be("MinimalPlayer");
        }

        [Fact]
        public async Task PlayerSpottedV1_WithSpecialCharacters_ShouldSerialize()
        {
            // Arrange
            var contract = new PlayerSpottedV1
            {
                EventId = "special-chars-123",
                ObservedAt = DateTimeOffset.Parse("2024-01-01T14:00:00Z"),
                Cluster = "Cluster with spaces",
                Region = "Region-with-dashes",
                PlayerId = 123,
                PlayerName = "Player with spaces & symbols!",
                GuildName = "Guild with 'quotes' and \"double quotes\"",
                AllianceName = "Alliance with émojis 🎮",
                X = -100.5f,
                Y = -200.3f,
                Tier = 7
            };

            // Act
            var json = JsonSerializer.Serialize(contract);
            var roundTrip = JsonSerializer.Deserialize<PlayerSpottedV1>(json);

            // Assert
            roundTrip.Should().NotBeNull();
            roundTrip!.PlayerName.Should().Contain("spaces");
            roundTrip.GuildName.Should().Contain("quotes");
        }

        [Fact]
        public async Task PlayerSpottedV1_WithExtremeValues_ShouldSerialize()
        {
            // Arrange
            var contract = new PlayerSpottedV1
            {
                EventId = "extreme-values",
                ObservedAt = DateTimeOffset.MaxValue,
                Cluster = "VeryLongClusterNameThatExceedsNormalLengthsAndTestsBoundaries",
                Region = "VeryLongRegionNameThatExceedsNormalLengthsAndTestsBoundaries",
                PlayerId = int.MaxValue,
                PlayerName = "VeryLongPlayerNameThatExceedsNormalLengthsAndTestsBoundaries",
                GuildName = "VeryLongGuildNameThatExceedsNormalLengthsAndTestsBoundaries",
                AllianceName = "VeryLongAllianceNameThatExceedsNormalLengthsAndTestsBoundaries",
                X = float.MaxValue,
                Y = float.MinValue,
                Tier = int.MaxValue
            };

            // Act
            var bytes = MessagePackSerializer.Serialize(contract);
            var roundTrip = MessagePackSerializer.Deserialize<PlayerSpottedV1>(bytes);

            // Assert
            roundTrip.Should().NotBeNull();
            roundTrip.PlayerId.Should().Be(int.MaxValue);
        }

        [Fact]
        public async Task PlayerSpottedV1_ContractStructure_ShouldBeValid()
        {
            // Arrange
            var contract = new PlayerSpottedV1
            {
                EventId = "ev",
                ObservedAt = default,
                Cluster = string.Empty,
                Region = string.Empty,
                PlayerId = 0,
                PlayerName = string.Empty
            };

            // Act & Assert - Verify the contract structure
            contract.Should().NotBeNull();
            contract.EventId.Should().NotBeNull();
            contract.PlayerName.Should().NotBeNull();
        }

        [Fact]
        public async Task PlayerSpottedV1_MessagePackSerialization_ShouldBeConsistent()
        {
            // Arrange
            var contract1 = new PlayerSpottedV1
            {
                EventId = "consistent-test-1",
                ObservedAt = DateTimeOffset.Parse("2024-01-01T15:00:00Z"),
                Cluster = "",
                Region = "",
                PlayerId = 12345,
                PlayerName = "ConsistentPlayer",
                X = 300.1f,
                Y = 400.2f,
                Tier = 5
            };

            var contract2 = new PlayerSpottedV1
            {
                EventId = "consistent-test-1",
                ObservedAt = DateTimeOffset.Parse("2024-01-01T15:00:00Z"),
                Cluster = "",
                Region = "",
                PlayerId = 12345,
                PlayerName = "ConsistentPlayer",
                X = 300.1f,
                Y = 400.2f,
                Tier = 5
            };

            // Act
            var s1 = MessagePackSerializer.Serialize(contract1);
            var s2 = MessagePackSerializer.Serialize(contract2);

            // Assert - Both should serialize identically
            s1.Should().NotBeNull();
            s1.Should().BeEquivalentTo(s2);
        }
    }
}
