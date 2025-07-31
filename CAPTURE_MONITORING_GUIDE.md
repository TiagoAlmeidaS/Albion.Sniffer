# 📊 Guia de Monitoramento de Captura de Pacotes

## 🎯 Visão Geral

O sistema de monitoramento de captura de pacotes foi implementado para fornecer **visibilidade completa** sobre o que está sendo interceptado pelo serviço de Capture. Ele inclui logging estruturado, métricas em tempo real e alertas automáticos.

---

## 🏗️ Componentes do Sistema

### 1. **PacketCaptureMetrics** 📈
Modelo que armazena todas as métricas de captura:
- Total de pacotes capturados
- Pacotes válidos vs descartados
- Taxa de pacotes/bytes por segundo
- Tempo de atividade
- Contadores de erro
- Status da captura

### 2. **PacketCaptureMonitor** 🔍
Serviço de monitoramento que:
- Registra eventos de captura
- Calcula métricas em tempo real
- Gera alertas automáticos
- Fornece logging estruturado
- Atualiza estatísticas periodicamente

### 3. **PacketCaptureService** (Atualizado) 🚀
Integração completa com monitoramento:
- Logging de dispositivos de rede
- Registro de pacotes capturados
- Tratamento de erros com contexto
- Métricas de performance

---

## 🚀 Como Usar

### **Inicialização Básica**

```csharp
// Criar logger
var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
var logger = loggerFactory.CreateLogger<PacketCaptureService>();

// Inicializar serviço com monitoramento
var captureService = new PacketCaptureService(5050, logger);

// Acessar monitor
var monitor = captureService.Monitor;

// Configurar eventos de métricas
monitor.OnMetricsUpdated += metrics => 
{
    Console.WriteLine($"📊 {metrics}");
};
```

### **Iniciando Captura com Monitoramento**

```csharp
try
{
    // Iniciar captura (logs automáticos incluídos)
    captureService.Start();
    
    // O monitor registrará automaticamente:
    // - Dispositivos de rede encontrados
    // - Filtros aplicados
    // - Início da captura
    // - Pacotes capturados em tempo real
}
catch (Exception ex)
{
    // Erros são automaticamente registrados no monitor
    Console.WriteLine($"Erro: {ex.Message}");
}
```

### **Monitoramento em Tempo Real**

```csharp
// Atualizar métricas manualmente
captureService.UpdateMetrics();

// Obter resumo detalhado
captureService.LogDetailedMetrics();

// Acessar métricas diretamente
var metrics = monitor.Metrics;
Console.WriteLine($"Pacotes capturados: {metrics.ValidPacketsCaptured}");
Console.WriteLine($"Taxa: {metrics.PacketsPerSecond:F2} pkt/s");
Console.WriteLine($"Status: {metrics.Status}");
```

---

## 📋 Tipos de Logs Gerados

### **1. Logs de Inicialização**
```
2025-01-29 15:30:00.123 [INF] PacketCaptureService inicializado - Porta: 5050, Filtro: udp and port 5050
2025-01-29 15:30:00.125 [INF] Verificando drivers de captura de pacotes...
2025-01-29 15:30:00.127 [INF] Sistema operacional: Linux
2025-01-29 15:30:00.130 [INF] Dispositivos de rede disponíveis: 3
```

### **2. Logs de Dispositivos**
```
2025-01-29 15:30:00.135 [DBG] Dispositivo 0: Ethernet (eth0)
2025-01-29 15:30:00.137 [DBG] Dispositivo 1: Loopback (lo)
2025-01-29 15:30:00.139 [DBG] Dispositivo 2: Wi-Fi (wlan0)
2025-01-29 15:30:00.141 [DBG] Dispositivo válido (MAC): Ethernet
```

### **3. Logs de Captura**
```
2025-01-29 15:30:00.150 [INF] Iniciando captura em 2 dispositivos válidos
2025-01-29 15:30:00.155 [INF] Captura iniciada - Interface: Ethernet, Filtro: udp and port 5050
2025-01-29 15:30:00.160 [INF] Filtro aplicado - Dispositivo: Ethernet, Filtro: udp and port 5050
```

### **4. Logs de Pacotes** (Debug/Trace)
```
2025-01-29 15:30:01.200 [DBG] Pacote capturado - Tamanho: 256 bytes, Total: 1
2025-01-29 15:30:01.201 [DBG] Pacote UDP capturado - Origem: 192.168.1.100:5050, Destino: 192.168.1.200:5050, Tamanho: 256 bytes
2025-01-29 15:30:01.202 [TRC] Pacote hex preview: F001010001000000000203...
```

### **5. Logs de Estatísticas** (Periódicos)
```
2025-01-29 15:30:05.000 [INF] Estatísticas de captura: Status: Running | Packets: 45/47 | Rate: 9.00 pkt/s, 2304.00 B/s | Dropped: 2 | Errors: 0 | Uptime: 00:00:05
```

### **6. Logs de Alertas**
```
2025-01-29 15:30:35.000 [WRN] Taxa de captura baixa - Nenhum pacote capturado nos últimos 30 segundos
2025-01-29 15:31:00.000 [WRN] Taxa de captura muito baixa: 0.05 pacotes/segundo
```

### **7. Logs de Erro**
```
2025-01-29 15:30:00.500 [ERR] Erro na captura de pacotes - Contexto: Device_OnPacketArrival, Total de erros: 1
System.ArgumentException: Invalid packet format
   at PacketDotNet.Packet.ParsePacket(...)
```

---

## ⚙️ Configuração de Logging

### **appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AlbionOnlineSniffer.Capture": "Debug",
      "AlbionOnlineSniffer.Capture.Services.PacketCaptureMonitor": "Information"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss.fff "
    }
  },
  "PacketMonitoring": {
    "EnableDetailedLogging": true,
    "EnableHexDump": false,
    "MetricsUpdateInterval": 5,
    "LogPeriodicStats": true,
    "AlertOnLowCaptureRate": true,
    "LowCaptureRateThreshold": 0.1
  }
}
```

### **Níveis de Log Recomendados**

| Cenário | Nível | Descrição |
|---------|-------|-----------|
| **Produção** | `Information` | Logs essenciais, estatísticas, alertas |
| **Debug** | `Debug` | + Detalhes de pacotes, dispositivos |
| **Desenvolvimento** | `Trace` | + Hex dumps, logs detalhados |

---

## 📊 Métricas Disponíveis

### **Métricas Básicas**
- `TotalPacketsCaptured`: Total de pacotes interceptados
- `ValidPacketsCaptured`: Pacotes UDP válidos na porta correta
- `PacketsDropped`: Pacotes descartados (inválidos)
- `TotalBytesCapturated`: Total de bytes processados

### **Métricas de Performance**
- `PacketsPerSecond`: Taxa de pacotes por segundo
- `BytesPerSecond`: Taxa de bytes por segundo
- `TotalCaptureTime`: Tempo total de captura ativa

### **Métricas de Qualidade**
- `CaptureErrors`: Número de erros de captura
- `LastError`: Última mensagem de erro
- `LastErrorTime`: Timestamp do último erro
- `Status`: Status atual ("Running", "Stopped", etc.)

### **Informações de Contexto**
- `LastInterface`: Última interface de rede utilizada
- `LastFilter`: Último filtro aplicado
- `LastCaptureTime`: Timestamp da última captura

---

## 🔧 API de Monitoramento

### **Métodos Disponíveis**

```csharp
// Forçar atualização de métricas
captureService.UpdateMetrics();

// Log detalhado das métricas
captureService.LogDetailedMetrics();

// Acessar métricas diretamente
var metrics = captureService.Monitor.Metrics;

// Configurar eventos
captureService.Monitor.OnMetricsUpdated += OnMetricsChanged;

// Logs manuais
captureService.Monitor.LogCaptureError(exception, "contexto");
captureService.Monitor.LogPeriodicStats();
```

### **Eventos Disponíveis**

```csharp
// Evento de atualização de métricas (a cada 5 segundos)
monitor.OnMetricsUpdated += (metrics) => 
{
    // Processar métricas atualizadas
    UpdateUI(metrics);
    CheckAlerts(metrics);
};
```

---

## 🚨 Sistema de Alertas

### **Alertas Automáticos**

1. **Taxa de Captura Baixa**
   - Dispara se não houver pacotes por 30+ segundos
   - Útil para detectar problemas de rede

2. **Taxa Muito Baixa**
   - Dispara se < 0.1 pacotes/segundo após 1 minuto
   - Indica possível problema de configuração

3. **Erros de Captura**
   - Log imediato de qualquer erro
   - Inclui contexto e stack trace

### **Alertas Customizados**

```csharp
monitor.OnMetricsUpdated += (metrics) => 
{
    // Alerta personalizado: alta taxa de descarte
    if (metrics.PacketsDropped > metrics.ValidPacketsCaptured * 0.1)
    {
        logger.LogWarning("Alta taxa de descarte: {DropRate:P}", 
            (double)metrics.PacketsDropped / metrics.TotalPacketsCaptured);
    }
    
    // Alerta personalizado: uso de memória
    if (metrics.TotalBytesCapturated > 100_000_000) // 100MB
    {
        logger.LogWarning("Alto uso de dados: {DataSize:N0} bytes capturados", 
            metrics.TotalBytesCapturated);
    }
};
```

---

## 📈 Exemplo de Saída Completa

```
2025-01-29 15:30:00.123 [INF] PacketCaptureService inicializado - Porta: 5050
2025-01-29 15:30:00.125 [INF] PacketCaptureMonitor inicializado
2025-01-29 15:30:00.130 [INF] Dispositivos de rede encontrados: 3
2025-01-29 15:30:00.135 [INF] Iniciando captura em 2 dispositivos válidos
2025-01-29 15:30:00.140 [INF] Captura iniciada - Interface: eth0, Filtro: udp and port 5050
2025-01-29 15:30:01.200 [DBG] Pacote capturado - Tamanho: 256 bytes, Total: 1
2025-01-29 15:30:01.250 [DBG] Pacote capturado - Tamanho: 128 bytes, Total: 2
2025-01-29 15:30:05.000 [INF] Estatísticas de captura: Status: Running | Packets: 45/47 | Rate: 9.00 pkt/s, 2304.00 B/s | Dropped: 2 | Errors: 0 | Uptime: 00:00:05
2025-01-29 15:30:10.000 [INF] Estatísticas de captura: Status: Running | Packets: 89/92 | Rate: 8.90 pkt/s, 2276.80 B/s | Dropped: 3 | Errors: 0 | Uptime: 00:00:10

=== RESUMO DETALHADO DE CAPTURA ===
Status: Running
Interface: eth0
Filtro: udp and port 5050
Tempo ativo: 00:01:00
Pacotes totais: 540
Pacotes válidos: 535
Pacotes descartados: 5
Bytes capturados: 137,280
Taxa de pacotes: 8.92 pkt/s
Taxa de bytes: 2,288.00 B/s
Erros de captura: 0
=== FIM DO RESUMO ===
```

---

## 🎯 Benefícios do Sistema

### **1. Visibilidade Completa** 👁️
- Monitoramento em tempo real de tudo que é capturado
- Métricas detalhadas de performance
- Histórico de erros e problemas

### **2. Debugging Facilitado** 🐛
- Logs estruturados com contexto
- Hex dumps opcionais para análise profunda
- Rastreamento de dispositivos e filtros

### **3. Alertas Proativos** 🚨
- Detecção automática de problemas
- Alertas configuráveis
- Monitoramento de saúde do sistema

### **4. Performance Otimizada** ⚡
- Logging assíncrono para não impactar captura
- Métricas calculadas em background
- Configuração flexível de níveis de log

### **5. Integração Fácil** 🔧
- API simples e intuitiva
- Configuração via appsettings.json
- Compatível com sistema de logging existente

---

## 📝 Resumo de Uso

```csharp
// 1. Inicializar com logging
var captureService = new PacketCaptureService(5050, logger);

// 2. Configurar monitoramento
captureService.Monitor.OnMetricsUpdated += metrics => 
    Console.WriteLine($"📊 {metrics}");

// 3. Iniciar captura (logs automáticos)
captureService.Start();

// 4. Monitorar em tempo real
captureService.UpdateMetrics();
captureService.LogDetailedMetrics();

// 5. Finalizar (logs de resumo automáticos)
captureService.Dispose();
```

O sistema agora fornece **visibilidade completa** sobre a interceptação de pacotes, facilitando debugging, monitoramento e otimização do processo de captura! 🎉