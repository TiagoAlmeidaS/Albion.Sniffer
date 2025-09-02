# 🔍 Discovery Service - Estatísticas em Tempo Real

## 📊 Visão Geral

O `DiscoveryService` agora inclui um sistema de estatísticas em tempo real que permite monitorar e analisar os pacotes descobertos pelo sniffer. As estatísticas são exibidas no console e atualizadas a cada 5 segundos.

## 🎯 Funcionalidades

### 📈 Estatísticas Gerais
- **Total de pacotes** processados
- **Pacotes visíveis** (processados)
- **Pacotes escondidos** (ignorados)
- **Percentuais** de cada categoria
- **Tempo de execução** do serviço

### 🏆 Top 10 Pacotes Mais Frequentes
- **Código** do pacote
- **Tipo** do pacote
- **Contagem** de ocorrências
- **Status** (visível/escondido)
- **Última ocorrência** (timestamp)

### 📊 Top 5 Tipos de Pacotes
- **Tipo** do pacote
- **Contagem** total por tipo

## 🚀 Como Usar

### 1. Iniciar o Sniffer
```bash
dotnet run --project src/AlbionOnlineSniffer.App
```

### 2. Monitorar Estatísticas
O console será limpo e atualizado a cada 5 segundos com as estatísticas mais recentes:

```
🔍 DISCOVERY SERVICE - ESTATÍSTICAS EM TEMPO REAL
============================================================
⏱️  Tempo de execução: 00:05:23
📊 Total de pacotes: 1,247
👁️  Pacotes visíveis: 892 (71.5%)
🙈 Pacotes escondidos: 355 (28.5%)

🏆 TOP 10 PACOTES MAIS FREQUENTES:
------------------------------------------------------------
Código   Tipo                 Count    Status     Último      
------------------------------------------------------------
123      MoveEvent            156      👁️  Visível  14:23:45
456      HealthUpdateEvent    89       👁️  Visível  14:23:44
789      NewCharacterEvent    67       👁️  Visível  14:23:43
321      LeaveEvent           45       🙈 Escondido 14:23:42
654      KeySyncEvent         34       👁️  Visível  14:23:41
...

📈 TOP 5 TIPOS DE PACOTES:
----------------------------------------
Tipo                     Count   
----------------------------------------
MoveEvent                156     
HealthUpdateEvent        89      
NewCharacterEvent        67      
LeaveEvent               45      
KeySyncEvent             34      

🔄 Atualizando a cada 5 segundos... (Ctrl+C para parar)
============================================================
```

## 🔧 Configuração

### Pacotes Escondidos
Os pacotes escondidos são definidos na lista `pacotesEscondidos` no `DiscoveryService.cs`:

```csharp
List<int> pacotesEscondidos = [
    1, 11, 19, 2, 3, 6, 21, 29, 35, 39, 40, 46, 47, 90, 91, 123, 209, 280, 319, 246, 359, 387, 514, 525, 526, 593
];
```

### Intervalo de Atualização
O intervalo de atualização das estatísticas pode ser modificado no construtor do `DiscoveryStatistics`:

```csharp
// Atualizar a cada 5 segundos (padrão)
_displayTimer = new Timer(DisplayStatistics, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

// Atualizar a cada 10 segundos
_displayTimer = new Timer(DisplayStatistics, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
```

## 🎯 Cenários de Investigação

### 1. **Novos Tipos de Pacotes**
- Monitore o "Top 10" para identificar pacotes com códigos desconhecidos
- Verifique se novos tipos aparecem na lista

### 2. **Pacotes Frequentes**
- Identifique pacotes que aparecem muito frequentemente
- Analise se são pacotes legítimos ou spam

### 3. **Pacotes Escondidos**
- Monitore quantos pacotes estão sendo escondidos
- Verifique se a lista de pacotes escondidos está adequada

### 4. **Padrões de Tráfego**
- Observe como os pacotes se comportam ao longo do tempo
- Identifique picos de atividade

### 5. **Anomalias**
- Procure por pacotes com contagens muito altas ou muito baixas
- Verifique se há tipos de pacotes inesperados

## 🛠️ Personalização

### Adicionar Novos Pacotes Escondidos
```csharp
List<int> pacotesEscondidos = [
    1, 11, 19, 2, 3, 6, 21, 29, 35, 39, 40, 46, 47, 90, 91, 123, 209, 280, 319, 246, 359, 387, 514, 525, 526, 593,
    // Adicionar novos códigos aqui
    600, 601, 602
];
```

### Modificar Formato de Exibição
Edite o método `DisplayStatistics` no `DiscoveryStatistics.cs` para personalizar a exibição.

### Adicionar Filtros
```csharp
// Filtrar apenas pacotes específicos
if (packetCode == 123 || packetCode == 456)
{
    _statistics.RecordPacket(packetCode, packetType, isHidden);
}
```

## 📝 Logs

- **Logs de debug** são silenciosos para não poluir o console
- **Estatísticas** são exibidas separadamente no console
- **Erros** são logados normalmente

## 🚨 Troubleshooting

### Console não atualiza
- Verifique se o `DiscoveryService` está sendo instanciado
- Confirme se o `DiscoveryDebugHandler` está conectado

### Estatísticas não aparecem
- Verifique se há pacotes sendo processados
- Confirme se o `OnPacketDecrypted` está sendo chamado

### Performance
- O sistema é otimizado para alta performance
- Usa `ConcurrentDictionary` para thread-safety
- Timer é eficiente e não bloqueia o fluxo principal
