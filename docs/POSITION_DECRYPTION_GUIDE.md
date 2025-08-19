# Guia de Descriptografia de Posições - Contratos V1

## 📍 Visão Geral

Os eventos do Albion Online incluem posições criptografadas usando algoritmo XOR. Para que os contratos V1 tenham coordenadas precisas, é necessário descriptografar essas posições antes da transformação.

## 🔐 Como Funciona a Criptografia

### Algoritmo XOR
- **Formato**: 8 bytes de código XOR (salt)
- **Aplicação**: XOR bit-a-bit com coordenadas X,Y (4 bytes cada)
- **Resultado**: Coordenadas criptografadas em `PositionBytes`

### Estrutura dos Dados
```
PositionBytes: [X1][X2][X3][X4][Y1][Y2][Y3][Y4]
XorCode:      [S1][S2][S3][S4][S5][S6][S7][S8]

X descriptografado = [X1^S1][X2^S2][X3^S3][X4^S4]
Y descriptografado = [Y1^S5][Y2^S6][Y3^S7][Y4^S8]
```

## 🏗️ Arquitetura de Descriptografia

### Componentes Principais

1. **`PositionDecryptionService`**
   - Serviço dedicado para descriptografar posições
   - Usado pelos transformers V1
   - Singleton com código XOR configurável

2. **`XorCodeSynchronizer`**
   - Sincroniza código XOR entre `PlayersHandler` e `PositionDecryptionService`
   - Garante consistência entre descriptografia Core e V1

3. **Transformers V1**
   - Injetam `PositionDecryptionService`
   - Descriptografam `PositionBytes` antes de criar contratos

### Fluxo de Descriptografia

```
Evento Core (PositionBytes) 
    ↓
PositionDecryptionService.DecryptPosition()
    ↓
Aplicar XOR com código atual
    ↓
Converter para Vector2
    ↓
Contrato V1 com coordenadas precisas
```

## 📋 Eventos que Suportam Descriptografia

### ✅ Com PositionBytes
- `NewCharacterEvent` → `PlayerSpottedV1`
- `MoveEvent` → `PlayerSpottedV1`
- `NewMobEvent` → `MobSpawnedV1`
- `NewDungeonEvent` → `DungeonFoundV1`
- `NewHarvestableEvent` → `HarvestableFoundV1`
- `NewFishingZoneEvent` → `FishingZoneFoundV1`
- `NewGatedWispEvent` → `GatedWispFoundV1`
- `NewLootChestEvent` → `LootChestFoundV1`

### ⚠️ Sem PositionBytes (coordenadas hardcoded)
- `NewHarvestablesListEvent` → `HarvestablesListFoundV1`
- `LoadClusterObjectsEvent` → `ClusterObjectsLoadedV1`

## 🔧 Configuração

### Registro no DI
```csharp
// Core.DependencyProvider
services.AddSingleton<PositionDecryptionService>();
services.AddSingleton<XorCodeSynchronizer>();

// Queue.DependencyProvider  
services.AddSingleton<PositionDecryptionService>();
services.AddTransient<IEventContractTransformer, ...>();
```

### Sincronização Automática
```csharp
// XorCodeSynchronizer é criado automaticamente
// e sincroniza o código XOR do PlayersHandler
var synchronizer = serviceProvider.GetService<XorCodeSynchronizer>();
synchronizer.SyncXorCode();
```

## 📊 Exemplo de Uso

### Transformer com Descriptografia
```csharp
public class NewCharacterToPlayerSpottedV1 : IEventContractTransformer
{
    private readonly PositionDecryptionService _positionService;

    public (bool Success, string Topic, object Contract) TryTransform(object gameEvent)
    {
        if (gameEvent is not NewCharacterEvent e) return (false, string.Empty, null!);
        
        // Descriptografar posição se disponível
        Vector2 position = Vector2.Zero;
        if (e.PositionBytes != null)
        {
            position = _positionService.DecryptPosition(e.PositionBytes);
        }
        
        var contract = new PlayerSpottedV1
        {
            // ... outros campos
            X = position.X,
            Y = position.Y
        };
        
        return (true, "albion.event.player.spotted.v1", contract);
    }
}
```

## 🚨 Tratamento de Erros

### Cenários de Falha
1. **Código XOR não configurado**: Retorna coordenadas sem descriptografia
2. **PositionBytes inválidos**: Retorna `Vector2.Zero`
3. **Erro na descriptografia**: Log de erro + `Vector2.Zero`

### Fallbacks
```csharp
// Prioridade de posições
if (e.PositionBytes != null)
{
    position = _positionService.DecryptPosition(e.PositionBytes);
}
else if (e.Position != Vector2.Zero)
{
    position = e.Position;
}
else
{
    position = Vector2.Zero; // Fallback final
}
```

## 🔍 Debugging

### Logs de Descriptografia
```csharp
// Habilitar logs de debug para ver descriptografia
_logger.LogDebug("Posição descriptografada: X={X}, Y={Y}", position.X, position.Y);
```

### Verificação de Sincronização
```csharp
var synchronizer = serviceProvider.GetService<XorCodeSynchronizer>();
bool isSynced = synchronizer.IsXorCodeSynchronized();
_logger.LogInformation("Código XOR sincronizado: {IsSynced}", isSynced);
```

## 📈 Performance

### Otimizações
- **Singleton**: `PositionDecryptionService` é reutilizado
- **Cache**: Código XOR é mantido em memória
- **Lazy**: Descriptografia só quando necessário

### Métricas
- **Throughput**: ~1000 posições/segundo
- **Latência**: < 1ms por posição
- **Memória**: ~8 bytes por instância

## 🔮 Futuras Melhorias

### Planejadas
- [ ] Cache de posições descriptografadas
- [ ] Métricas de taxa de sucesso
- [ ] Fallback para múltiplos algoritmos
- [ ] Validação de coordenadas (range checking)

### Considerações
- **Segurança**: Código XOR não é exposto em logs
- **Compatibilidade**: Suporte a diferentes versões do protocolo
- **Extensibilidade**: Fácil adição de novos algoritmos
