using Albion.Events.V1;
using Bogus;

namespace AlbionOnlineSniffer.Tests.Common.Builders;

/// <summary>
/// Builder para criar eventos de domínio com dados sintéticos
/// </summary>
public class DomainEventBuilder
{
    private readonly Faker _faker = new Faker();
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;

    public DomainEventBuilder(IClock? clock = null, IIdGenerator? idGenerator = null)
    {
        _clock = clock ?? new FakeClock();
        _idGenerator = idGenerator ?? new FakeIdGenerator();
    }

    public PlayerSpottedV1 BuildPlayerSpottedV1(
        string? name = null,
        string? guild = null,
        string? alliance = null,
        int? tier = null,
        float? x = null,
        float? y = null,
        float? z = null,
        double? distanceMeters = null)
    {
        return new PlayerSpottedV1(
            EventId: _idGenerator.NewGuid(),
            Timestamp: _clock.UtcNow,
            Name: name ?? _faker.Internet.UserName(),
            Guild: guild ?? _faker.Company.CompanyName(),
            Alliance: alliance ?? _faker.Company.CompanyName(),
            Tier: tier ?? _faker.Random.Int(1, 8),
            X: x ?? _faker.Random.Float(-1000, 1000),
            Y: y ?? _faker.Random.Float(-1000, 1000),
            Z: z ?? _faker.Random.Float(0, 100),
            DistanceMeters: distanceMeters ?? _faker.Random.Double(0, 5000)
        );
    }

    public MobSpawnedV1 BuildMobSpawnedV1(
        string? mobType = null,
        int? tier = null,
        float? x = null,
        float? y = null,
        float? z = null,
        int? health = null,
        int? maxHealth = null)
    {
        return new MobSpawnedV1(
            EventId: _idGenerator.NewGuid(),
            Timestamp: _clock.UtcNow,
            MobType: mobType ?? _faker.PickRandom("Wolf", "Bear", "Boar", "Rabbit", "Deer"),
            Tier: tier ?? _faker.Random.Int(1, 8),
            X: x ?? _faker.Random.Float(-1000, 1000),
            Y: y ?? _faker.Random.Float(-1000, 1000),
            Z: z ?? _faker.Random.Float(0, 100),
            Health: health ?? _faker.Random.Int(100, 10000),
            MaxHealth: maxHealth ?? _faker.Random.Int(100, 10000)
        );
    }

    public HarvestableFoundV1 BuildHarvestableFoundV1(
        string? resourceType = null,
        int? tier = null,
        float? x = null,
        float? y = null,
        float? z = null,
        int? charges = null)
    {
        return new HarvestableFoundV1(
            EventId: _idGenerator.NewGuid(),
            Timestamp: _clock.UtcNow,
            ResourceType: resourceType ?? _faker.PickRandom("Wood", "Stone", "Ore", "Fiber", "Hide"),
            Tier: tier ?? _faker.Random.Int(1, 8),
            X: x ?? _faker.Random.Float(-1000, 1000),
            Y: y ?? _faker.Random.Float(-1000, 1000),
            Z: z ?? _faker.Random.Float(0, 100),
            Charges: charges ?? _faker.Random.Int(1, 10)
        );
    }

    public LootChestFoundV1 BuildLootChestFoundV1(
        string? chestType = null,
        float? x = null,
        float? y = null,
        float? z = null,
        string? rarity = null)
    {
        return new LootChestFoundV1(
            EventId: _idGenerator.NewGuid(),
            Timestamp: _clock.UtcNow,
            ChestType: chestType ?? _faker.PickRandom("Common", "Uncommon", "Rare", "Legendary"),
            X: x ?? _faker.Random.Float(-1000, 1000),
            Y: y ?? _faker.Random.Float(-1000, 1000),
            Z: z ?? _faker.Random.Float(0, 100),
            Rarity: rarity ?? _faker.PickRandom("Common", "Uncommon", "Rare", "Legendary")
        );
    }

    public ClusterChangedV1 BuildClusterChangedV1(
        string? fromCluster = null,
        string? toCluster = null,
        string? zoneType = null,
        int? tier = null)
    {
        return new ClusterChangedV1(
            EventId: _idGenerator.NewGuid(),
            Timestamp: _clock.UtcNow,
            FromCluster: fromCluster ?? _faker.Address.City(),
            ToCluster: toCluster ?? _faker.Address.City(),
            ZoneType: zoneType ?? _faker.PickRandom("Blue", "Yellow", "Red", "Black"),
            Tier: tier ?? _faker.Random.Int(1, 8)
        );
    }

    public HealthUpdatedV1 BuildHealthUpdatedV1(
        string? entityId = null,
        int? currentHealth = null,
        int? maxHealth = null,
        float? healthPercentage = null)
    {
        var current = currentHealth ?? _faker.Random.Int(0, 10000);
        var max = maxHealth ?? _faker.Random.Int(current, 10000);
        
        return new HealthUpdatedV1(
            EventId: _idGenerator.NewGuid(),
            Timestamp: _clock.UtcNow,
            EntityId: entityId ?? _idGenerator.NewGuid().ToString(),
            CurrentHealth: current,
            MaxHealth: max,
            HealthPercentage: healthPercentage ?? (float)current / max * 100
        );
    }
}