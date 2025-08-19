using System.Text.Json;
using Albion.Events.V1;
using MessagePack;
using VerifyTests;
using VerifyXunit;
using VerifyTests;
using Xunit;

namespace AlbionOnlineSniffer.Tests.Contracts;

public class PlayerSpottedV1_SnapshotTests
{
    [Fact]
    public async Task PlayerSpottedV1_Serializes_Consistently()
    {
        var evt = new PlayerSpottedV1
        {
            EventId = "evt-123",
            ObservedAt = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero),
            Cluster = "lymhurst",
            Region = "forest",
            PlayerId = 42,
            PlayerName = "JohnDoe",
            GuildName = "GuildX",
            AllianceName = "AllianceY",
            X = 123.45f,
            Y = 67.89f,
            Tier = 6
        };

        // JSON snapshot (human readable)
        var json = JsonSerializer.Serialize(evt, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // MessagePack snapshot (binary) – use Verify.MessagePack extension
        var mp = MessagePackSerializer.Serialize(evt);

        await Verify(new
        {
            json,
            messagePack = mp
        });
    }
}

