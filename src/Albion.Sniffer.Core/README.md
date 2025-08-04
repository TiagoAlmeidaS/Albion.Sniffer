# Albion.Sniffer.Core

[![NuGet](https://img.shields.io/nuget/v/Albion.Sniffer.Core.svg)](https://www.nuget.org/packages/Albion.Sniffer.Core/)
[![Downloads](https://img.shields.io/nuget/dt/Albion.Sniffer.Core.svg)](https://www.nuget.org/packages/Albion.Sniffer.Core/)

Core library for Albion Online packet parsing, event processing and game data management. Provides essential services, parsers, and event dispatching for Albion Online network analysis.

## 🚀 Features

- **Protocol16Deserializer**: Advanced Photon packet parsing and deserialization
- **EventDispatcher**: Robust event management system for game events
- **Comprehensive Event Handlers**: Support for all major Albion Online game events
- **Data Services**: Built-in services for items, clusters, and game objects
- **Position & Data Decryption**: Utilities for decrypting game data
- **Complete Event Models**: Strongly-typed models for all game events
- **Game Object Managers**: Handlers for players, mobs, harvestables, and more

## 📦 Installation

### Package Manager Console
```powershell
Install-Package Albion.Sniffer.Core
```

### .NET CLI
```bash
dotnet add package Albion.Sniffer.Core
```

### PackageReference
```xml
<PackageReference Include="Albion.Sniffer.Core" Version="1.0.0" />
```

## 🔧 Quick Start

### Basic Setup

```csharp
using Albion.Sniffer.Core.Services;
using Albion.Sniffer.Core.Models.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Register core services
services.AddSingleton<EventDispatcher>();
services.AddSingleton<Protocol16Deserializer>();
services.AddSingleton<DataLoaderService>();

var serviceProvider = services.BuildServiceProvider();
```

### Event Handling

```csharp
var eventDispatcher = serviceProvider.GetService<EventDispatcher>();

// Subscribe to player movement events
eventDispatcher.Subscribe<MoveEvent>(moveEvent => 
{
    Console.WriteLine($"Player {moveEvent.ObjectId} moved to {moveEvent.Position}");
});

// Subscribe to new character events
eventDispatcher.Subscribe<NewCharacterEvent>(characterEvent => 
{
    Console.WriteLine($"New character: {characterEvent.Name} from guild {characterEvent.Guild}");
});

// Subscribe to cluster change events
eventDispatcher.Subscribe<ChangeClusterEvent>(clusterEvent => 
{
    Console.WriteLine($"Changed to cluster: {clusterEvent.LocationId}");
});
```

### Packet Processing

```csharp
var deserializer = serviceProvider.GetService<Protocol16Deserializer>();

// Process incoming Photon packets
byte[] packetData = GetPhotonPacketData(); // Your packet capture logic
var deserializedData = deserializer.Deserialize(packetData);

// The EventDispatcher will automatically process and dispatch events
```

## 📋 Supported Events

### Player Events
- `NewCharacterEvent` - New player joined
- `MoveEvent` - Player movement
- `MountedEvent` - Player mounted/dismounted
- `CharacterEquipmentChangedEvent` - Equipment changes
- `HealthUpdateEvent` - Health changes

### World Events
- `ChangeClusterEvent` - Zone/cluster changes
- `LoadClusterObjectsEvent` - Cluster objects loaded
- `LeaveEvent` - Player left area

### Object Events
- `NewMobEvent` - New mob spawned
- `MobChangeStateEvent` - Mob state changed
- `NewHarvestableEvent` - New harvestable resource
- `HarvestableChangeStateEvent` - Resource state changed
- `NewLootChestEvent` - New loot chest
- `NewDungeonEvent` - New dungeon entrance
- `NewFishingZoneEvent` - New fishing zone
- `NewGatedWispEvent` - New wisp gate

### System Events
- `KeySyncEvent` - Encryption key synchronization
- `RegenerationChangedEvent` - Regeneration changes
- `WispGateOpenedEvent` - Wisp gate opened

## 🏗️ Architecture

### Core Services

- **EventDispatcher**: Central event management and subscription system
- **Protocol16Deserializer**: Handles Photon Protocol 16 packet deserialization
- **DataLoaderService**: Loads and manages game data (items, clusters, etc.)
- **ItemDataService**: Provides item information and metadata
- **ClusterService**: Manages cluster/zone information
- **PositionDecryptor**: Decrypts encrypted position data
- **XorDecryptor**: Handles XOR-based data decryption

### Game Object Handlers

- **PlayersHandler**: Manages player objects and state
- **MobsHandler**: Handles mob/NPC objects
- **HarvestablesHandler**: Manages harvestable resources
- **LocalPlayerHandler**: Tracks local player state
- **DungeonsHandler**: Handles dungeon objects
- **LootChestsHandler**: Manages loot chest objects

## 🔌 Integration Examples

### Bot Integration

```csharp
public class AlbionBot
{
    private readonly EventDispatcher _eventDispatcher;
    
    public AlbionBot(EventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
        SetupEventHandlers();
    }
    
    private void SetupEventHandlers()
    {
        _eventDispatcher.Subscribe<NewHarvestableEvent>(OnNewHarvestable);
        _eventDispatcher.Subscribe<NewMobEvent>(OnNewMob);
        _eventDispatcher.Subscribe<MoveEvent>(OnPlayerMove);
    }
    
    private void OnNewHarvestable(NewHarvestableEvent harvestableEvent)
    {
        // Bot logic for new harvestable resources
        if (harvestableEvent.Type == "T8_ROCK")
        {
            NavigateToPosition(harvestableEvent.Position);
        }
    }
}
```

### Radar/Map Integration

```csharp
public class AlbionRadar
{
    private readonly Dictionary<int, PlayerInfo> _players = new();
    private readonly EventDispatcher _eventDispatcher;
    
    public AlbionRadar(EventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
        _eventDispatcher.Subscribe<NewCharacterEvent>(OnNewCharacter);
        _eventDispatcher.Subscribe<MoveEvent>(OnPlayerMove);
        _eventDispatcher.Subscribe<LeaveEvent>(OnPlayerLeave);
    }
    
    private void OnNewCharacter(NewCharacterEvent characterEvent)
    {
        _players[characterEvent.ObjectId] = new PlayerInfo
        {
            Name = characterEvent.Name,
            Guild = characterEvent.Guild,
            Position = characterEvent.Position
        };
        
        UpdateRadarDisplay();
    }
}
```

## 🛠️ Advanced Usage

### Custom Event Handlers

```csharp
public class CustomEventHandler : IEventHandler<NewCharacterEvent>
{
    public void Handle(NewCharacterEvent eventData)
    {
        // Custom processing logic
        ProcessPlayerData(eventData);
    }
}

// Register custom handler
eventDispatcher.Subscribe<NewCharacterEvent>(new CustomEventHandler());
```

### Data Access

```csharp
var dataLoader = serviceProvider.GetService<DataLoaderService>();
var itemService = serviceProvider.GetService<ItemDataService>();

// Access cluster information
var clusterData = dataLoader.GetClusterData("cluster_id");

// Get item information
var itemInfo = itemService.GetItemInfo("T8_2H_SWORD");
```

## 📖 Dependencies

- **.NET 8.0**: Target framework
- **Albion.Network 5.0.1**: Albion Online network protocol support
- **Microsoft.Extensions.Logging**: Logging infrastructure
- **PacketDotNet 1.4.8**: Network packet processing

## 🤝 Contributing

This library is part of the Albion.Sniffer ecosystem. For contributions, issues, or feature requests, please visit the main repository:

[https://github.com/TiagoAlmeidaS/Albion.Sniffer](https://github.com/TiagoAlmeidaS/Albion.Sniffer)

## 📄 License

This project is licensed under the MIT License - see the main repository for details.

## 🔗 Related Projects

- **Albion.Sniffer.App**: Main application using this core library
- **Albion.Sniffer.Capture**: Network packet capture functionality
- **Albion.Sniffer.Queue**: Message queue processing

---

Made with ❤️ for the Albion Online community