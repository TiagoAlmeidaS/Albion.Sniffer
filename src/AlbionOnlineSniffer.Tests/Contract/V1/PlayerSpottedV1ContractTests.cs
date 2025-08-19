using System;
using System.Threading.Tasks;
using VerifyXunit;
using VerifyTests;
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
                PlayerId = "player-456",
                PlayerName = "TestPlayer",
                GuildName = "TestGuild",
                AllianceName = "TestAlliance",
                X = 100.5f,
                Y = 200.3f,
                Tier = 6
            };

            // Act & Assert
            await Verify(contract)
                .UseDirectory("Snapshots")
                .UseFileName("PlayerSpottedV1_MessagePack");
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
                PlayerId = "player-789",
                PlayerName = "AnotherPlayer",
                GuildName = null, // Test null value
                AllianceName = null, // Test null value
                X = 150.7f,
                Y = 250.9f,
                Tier = 8
            };

            // Act & Assert
            await Verify(contract)
                .UseDirectory("Snapshots")
                .UseFileName("PlayerSpottedV1_JSON");
        }

        [Fact]
        public async Task PlayerSpottedV1_WithMinimalData_ShouldSerialize()
        {
            // Arrange
            var contract = new PlayerSpottedV1
            {
                EventId = "minimal-event",
                ObservedAt = DateTimeOffset.UtcNow,
                PlayerId = "minimal-player",
                PlayerName = "MinimalPlayer",
                X = 0f,
                Y = 0f,
                Tier = 0
            };

            // Act & Assert
            await Verify(contract)
                .UseDirectory("Snapshots")
                .UseFileName("PlayerSpottedV1_Minimal");
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
                PlayerId = "player_special_123",
                PlayerName = "Player with spaces & symbols!",
                GuildName = "Guild with 'quotes' and \"double quotes\"",
                AllianceName = "Alliance with émojis 🎮",
                X = -100.5f,
                Y = -200.3f,
                Tier = 7
            };

            // Act & Assert
            await Verify(contract)
                .UseDirectory("Snapshots")
                .UseFileName("PlayerSpottedV1_SpecialCharacters");
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
                PlayerId = "very-long-player-id-that-exceeds-normal-lengths-and-tests-boundaries",
                PlayerName = "VeryLongPlayerNameThatExceedsNormalLengthsAndTestsBoundaries",
                GuildName = "VeryLongGuildNameThatExceedsNormalLengthsAndTestsBoundaries",
                AllianceName = "VeryLongAllianceNameThatExceedsNormalLengthsAndTestsBoundaries",
                X = float.MaxValue,
                Y = float.MinValue,
                Tier = int.MaxValue
            };

            // Act & Assert
            await Verify(contract)
                .UseDirectory("Snapshots")
                .UseFileName("PlayerSpottedV1_ExtremeValues");
        }

        [Fact]
        public async Task PlayerSpottedV1_ContractStructure_ShouldBeValid()
        {
            // Arrange
            var contract = new PlayerSpottedV1();

            // Act & Assert - Verify the contract structure
            contract.Should().NotBeNull();
            contract.EventId.Should().BeNull();
            contract.ObservedAt.Should().Be(default(DateTimeOffset));
            contract.Cluster.Should().BeNull();
            contract.Region.Should().BeNull();
            contract.PlayerId.Should().BeNull();
            contract.PlayerName.Should().BeNull();
            contract.GuildName.Should().BeNull();
            contract.AllianceName.Should().BeNull();
            contract.X.Should().Be(0f);
            contract.Y.Should().Be(0f);
            contract.Tier.Should().Be(0);
        }

        [Fact]
        public async Task PlayerSpottedV1_MessagePackSerialization_ShouldBeConsistent()
        {
            // Arrange
            var contract1 = new PlayerSpottedV1
            {
                EventId = "consistent-test-1",
                ObservedAt = DateTimeOffset.Parse("2024-01-01T15:00:00Z"),
                PlayerId = "player-consistent",
                PlayerName = "ConsistentPlayer",
                X = 300.1f,
                Y = 400.2f,
                Tier = 5
            };

            var contract2 = new PlayerSpottedV1
            {
                EventId = "consistent-test-1",
                ObservedAt = DateTimeOffset.Parse("2024-01-01T15:00:00Z"),
                PlayerId = "player-consistent",
                PlayerName = "ConsistentPlayer",
                X = 300.1f,
                Y = 400.2f,
                Tier = 5
            };

            // Act & Assert - Both should serialize identically
            await Verify(contract1)
                .UseDirectory("Snapshots")
                .UseFileName("PlayerSpottedV1_Consistent1");

            await Verify(contract2)
                .UseDirectory("Snapshots")
                .UseFileName("PlayerSpottedV1_Consistent2");
        }
    }
}
