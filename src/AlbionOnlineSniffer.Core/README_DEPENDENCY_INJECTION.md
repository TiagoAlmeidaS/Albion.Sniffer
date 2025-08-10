# Injeção de Dependência para PacketOffsets

## Visão Geral

Foi implementado um sistema de injeção de dependência para resolver o acoplamento forte com `PacketOffsetsLoader.GlobalPacketOffsets` nos eventos. A solução mantém compatibilidade com o framework Albion.Network existente.

## Implementação

### 1. PacketOffsetsProvider (Service Locator)

Um provider estático que resolve PacketOffsets através do ServiceProvider:

```csharp
// Configurar o provider após construir o ServiceProvider
PacketOffsetsProvider.Configure(serviceProvider);

// Usar nos eventos
var packetOffsets = PacketOffsetsProvider.GetOffsets();
```

### 2. Eventos Atualizados

Todos os eventos foram atualizados para suportar duas formas de inicialização:

```csharp
// Construtor 1: Compatibilidade com Albion.Network (usa PacketOffsetsProvider)
public MoveEvent(Dictionary<byte, object> parameters) : base(parameters)

// Construtor 2: Injeção de dependência direta
public MoveEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
```

### 3. Configuração no DependencyProvider

Métodos adicionados para permitir sobrescrita de PacketOffsets:

```csharp
// Registro com parâmetros customizados
DependencyProvider.RegisterDataLoader(services, customPacketOffsets, customPacketIndexes);

// Sobrescrita completa
DependencyProvider.OverridePacketOffsets(services, customPacketOffsets);

// Configuração do provider
DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);
```

## Como Usar

### Setup Básico

```csharp
// 1. Configurar serviços
var services = new ServiceCollection();
DependencyProvider.RegisterServices(services);
var serviceProvider = services.BuildServiceProvider();

// 2. Configurar o PacketOffsetsProvider
DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);

// 3. Usar normalmente - os eventos resolverão automaticamente via provider
```

### Setup com PacketOffsets Customizado

```csharp
// Criar PacketOffsets customizado
var customOffsets = new PacketOffsets
{
    Move = new byte[] { 1, 2, 3 },
    ChangeCluster = new byte[] { 4, 5, 6 }
    // ... outros offsets
};

// Registrar com customização
var services = new ServiceCollection();
DependencyProvider.RegisterDataLoader(services, customOffsets);
DependencyProvider.RegisterServices(services);

var serviceProvider = services.BuildServiceProvider();
DependencyProvider.ConfigurePacketOffsetsProvider(serviceProvider);
```

### Sobrescrita Dinâmica

```csharp
// Para sobrescrever PacketOffsets em runtime
DependencyProvider.OverridePacketOffsets(services, newPacketOffsets);

// Forçar refresh do cache
PacketOffsetsProvider.RefreshOffsets();
```

## Benefícios

1. **Desacoplamento**: Eventos não dependem mais diretamente de PacketOffsetsLoader
2. **Flexibilidade**: Permite customização e sobrescrita de PacketOffsets
3. **Testabilidade**: Facilita testes unitários com mocks
4. **Compatibilidade**: Mantém funcionamento com framework Albion.Network existente
5. **Configurabilidade**: Permite diferentes configurações para diferentes cenários

## EventFactory (Alternativa)

Para casos avançados, uma EventFactory também está disponível:

```csharp
var eventFactory = serviceProvider.GetRequiredService<IEventFactory>();
var moveEvent = eventFactory.CreateEvent<MoveEvent>(parameters);
```

## Migração

A migração é transparente - todos os eventos existentes continuarão funcionando sem alterações no código que os usa. O framework Albion.Network continuará usando os construtores padrão dos eventos, que agora resolvem PacketOffsets via PacketOffsetsProvider.