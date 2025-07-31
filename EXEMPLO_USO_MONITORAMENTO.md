# 🚀 Exemplo Prático: Usando o Sistema de Monitoramento

## 📋 Cenário
Você quer monitorar em tempo real o que o seu PacketCaptureService está interceptando na porta 5050, com logs detalhados e métricas de performance.

---

## 💻 Código de Exemplo

### **1. Configuração Básica**

```csharp
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Capture;

// Configurar logging
var loggerFactory = LoggerFactory.Create(builder => 
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug) // Para ver logs detalhados
        .AddFilter("AlbionOnlineSniffer.Capture", LogLevel.Information));

var logger = loggerFactory.CreateLogger<Program>();

// Inicializar serviço de captura com monitoramento
var captureService = new PacketCaptureService(5050, logger);

logger.LogInformation("🚀 Iniciando sistema de monitoramento...");
```

### **2. Configurar Eventos de Monitoramento**

```csharp
// Configurar evento para receber atualizações de métricas a cada 5 segundos
captureService.Monitor.OnMetricsUpdated += (metrics) => 
{
    // Log das métricas principais
    Console.WriteLine($"📊 {metrics}");
    
    // Alertas customizados
    if (metrics.PacketsDropped > 10)
    {
        Console.WriteLine($"⚠️  ALERTA: {metrics.PacketsDropped} pacotes descartados!");
    }
    
    if (metrics.PacketsPerSecond > 50)
    {
        Console.WriteLine($"🔥 ALTA ATIVIDADE: {metrics.PacketsPerSecond:F2} pacotes/segundo");
    }
    
    if (metrics.CaptureErrors > 0)
    {
        Console.WriteLine($"❌ ERROS DETECTADOS: {metrics.CaptureErrors} erros de captura");
    }
};

// Configurar evento para receber pacotes capturados
captureService.OnUdpPayloadCaptured += (payload) => 
{
    logger.LogInformation("📦 Pacote interceptado e enviado para parser - {Size} bytes", payload.Length);
    
    // Aqui você pode processar o pacote ou enviá-lo para seu parser
    // ProcessarPacote(payload);
};
```

### **3. Iniciar Captura com Monitoramento**

```csharp
try
{
    // Iniciar captura - logs automáticos serão gerados
    captureService.Start();
    
    logger.LogInformation("✅ Captura iniciada com sucesso!");
    logger.LogInformation("🔍 Monitorando porta 5050...");
    logger.LogInformation("📊 Métricas serão atualizadas a cada 5 segundos");
    
    // Manter programa rodando
    Console.WriteLine("Pressione qualquer tecla para ver métricas detalhadas, ou 'q' para sair...");
    
    while (true)
    {
        var key = Console.ReadKey(true);
        
        if (key.KeyChar == 'q' || key.KeyChar == 'Q')
        {
            break;
        }
        else if (key.KeyChar == 'm' || key.KeyChar == 'M')
        {
            // Mostrar métricas detalhadas
            captureService.LogDetailedMetrics();
        }
        else if (key.KeyChar == 'u' || key.KeyChar == 'U')
        {
            // Forçar atualização de métricas
            captureService.UpdateMetrics();
        }
        else
        {
            // Mostrar resumo atual
            var metrics = captureService.Monitor.Metrics;
            Console.WriteLine($"\n📊 Status: {metrics.Status}");
            Console.WriteLine($"📦 Pacotes: {metrics.ValidPacketsCaptured}");
            Console.WriteLine($"⚡ Taxa: {metrics.PacketsPerSecond:F2} pkt/s");
            Console.WriteLine($"⏱️  Uptime: {metrics.TotalCaptureTime}");
            Console.WriteLine("\nComandos: [M]étricas detalhadas | [U]pdate | [Q]uit\n");
        }
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Erro ao iniciar captura: {Message}", ex.Message);
}
finally
{
    // Parar captura e mostrar resumo final
    captureService.Dispose();
    logger.LogInformation("🛑 Captura finalizada");
}
```

---

## 📊 Exemplo de Saída

### **Durante a Execução:**

```
2025-01-29 15:30:00.123 [INF] 🚀 Iniciando sistema de monitoramento...
2025-01-29 15:30:00.125 [INF] PacketCaptureService inicializado - Porta: 5050, Filtro: udp and port 5050
2025-01-29 15:30:00.127 [INF] PacketCaptureMonitor inicializado
2025-01-29 15:30:00.130 [INF] Verificando drivers de captura de pacotes...
2025-01-29 15:30:00.132 [INF] Sistema operacional: Linux
2025-01-29 15:30:00.135 [INF] Dispositivos de rede disponíveis: 3
2025-01-29 15:30:00.140 [DBG] Dispositivo 0: Ethernet (eth0)
2025-01-29 15:30:00.142 [DBG] Dispositivo 1: Loopback (lo)
2025-01-29 15:30:00.144 [DBG] Dispositivo 2: Wi-Fi (wlan0)
2025-01-29 15:30:00.146 [INF] Dispositivos de rede encontrados: 3
2025-01-29 15:30:00.150 [INF] Iniciando captura em 2 dispositivos válidos
2025-01-29 15:30:00.155 [INF] Captura iniciada no dispositivo: Ethernet
2025-01-29 15:30:00.157 [INF] Filtro aplicado - Dispositivo: Ethernet, Filtro: udp and port 5050
2025-01-29 15:30:00.160 [INF] Captura iniciada - Interface: Ethernet, Filtro: udp and port 5050
2025-01-29 15:30:00.162 [INF] ✅ Captura iniciada com sucesso!
2025-01-29 15:30:00.164 [INF] 🔍 Monitorando porta 5050...
2025-01-29 15:30:00.166 [INF] 📊 Métricas serão atualizadas a cada 5 segundos

Pressione qualquer tecla para ver métricas detalhadas, ou 'q' para sair...

2025-01-29 15:30:01.200 [DBG] Pacote capturado - Tamanho: 256 bytes, Total: 1
2025-01-29 15:30:01.201 [DBG] Pacote UDP capturado - Porta origem: 5050, Porta destino: 5050, Tamanho: 256 bytes
2025-01-29 15:30:01.202 [INF] 📦 Pacote interceptado e enviado para parser - 256 bytes

2025-01-29 15:30:01.250 [DBG] Pacote capturado - Tamanho: 128 bytes, Total: 2
2025-01-29 15:30:01.251 [INF] 📦 Pacote interceptado e enviado para parser - 128 bytes

2025-01-29 15:30:05.000 [INF] Estatísticas de captura: Status: Running | Packets: 45/47 | Rate: 9.00 pkt/s, 2304.00 B/s | Dropped: 2 | Errors: 0 | Uptime: 00:00:05
📊 Status: Running | Packets: 45/47 | Rate: 9.00 pkt/s, 2304.00 B/s | Dropped: 2 | Errors: 0 | Uptime: 00:00:05

2025-01-29 15:30:10.000 [INF] Estatísticas de captura: Status: Running | Packets: 89/92 | Rate: 8.90 pkt/s, 2276.80 B/s | Dropped: 3 | Errors: 0 | Uptime: 00:00:10
📊 Status: Running | Packets: 89/92 | Rate: 8.90 pkt/s, 2276.80 B/s | Dropped: 3 | Errors: 0 | Uptime: 00:00:10
🔥 ALTA ATIVIDADE: 8.90 pacotes/segundo
```

### **Métricas Detalhadas (Tecla 'M'):**

```
=== RESUMO DETALHADO DE CAPTURA ===
Status: Running
Interface: Ethernet
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

### **Finalização:**

```
2025-01-29 15:31:00.000 [INF] Captura de pacotes parada
2025-01-29 15:31:00.002 [INF] Captura parada - Estatísticas finais: Status: Stopped | Packets: 535/540 | Rate: 8.92 pkt/s, 2288.00 B/s | Dropped: 5 | Errors: 0 | Uptime: 00:01:00
2025-01-29 15:31:00.005 [INF] PacketCaptureMonitor finalizado
2025-01-29 15:31:00.007 [INF] PacketCaptureService finalizado
2025-01-29 15:31:00.009 [INF] 🛑 Captura finalizada
```

---

## ⚙️ Configuração Avançada

### **appsettings.json para Produção:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AlbionOnlineSniffer.Capture": "Information",
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

### **Para Debug/Desenvolvimento:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "AlbionOnlineSniffer.Capture": "Trace"
    }
  },
  "PacketMonitoring": {
    "EnableHexDump": true
  }
}
```

---

## 🔧 Integração com Seu Sistema

### **Processamento de Pacotes:**

```csharp
captureService.OnUdpPayloadCaptured += async (payload) => 
{
    try
    {
        // Enviar para seu parser Photon
        var parsedPacket = await photonParser.ParseAsync(payload);
        
        if (parsedPacket != null)
        {
            logger.LogInformation("✅ Pacote parseado: {PacketType}", parsedPacket.Type);
            
            // Enviar para fila
            await queuePublisher.PublishAsync("albion_packets", parsedPacket);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Erro ao processar pacote");
        captureService.Monitor.LogCaptureError(ex, "Processamento de pacote");
    }
};
```

### **Alertas Customizados:**

```csharp
captureService.Monitor.OnMetricsUpdated += (metrics) => 
{
    // Alerta de alta taxa de descarte
    var dropRate = (double)metrics.PacketsDropped / metrics.TotalPacketsCaptured;
    if (dropRate > 0.1) // Mais de 10% de descarte
    {
        logger.LogWarning("🚨 ALERTA: Alta taxa de descarte {DropRate:P}", dropRate);
        // Enviar notificação, email, etc.
    }
    
    // Alerta de baixa atividade
    if (metrics.PacketsPerSecond < 1.0 && metrics.TotalCaptureTime.TotalMinutes > 5)
    {
        logger.LogWarning("⚠️  ALERTA: Baixa atividade de rede detectada");
    }
    
    // Salvar métricas em banco de dados
    await SaveMetricsToDatabase(metrics);
};
```

---

## 📈 Benefícios Práticos

### **1. Debugging Facilitado** 🐛
- **Antes**: "Por que não estou recebendo pacotes?"
- **Depois**: "Status: Running | Packets: 0/0 | Rate: 0.00 pkt/s | Errors: 0" → Problema na rede/filtro

### **2. Monitoramento em Tempo Real** 📊
- **Antes**: Código rodando "no escuro"
- **Depois**: Visibilidade completa de tudo que está acontecendo

### **3. Alertas Proativos** 🚨
- **Antes**: Descobrir problemas só quando algo para de funcionar
- **Depois**: Alertas automáticos antes que problemas afetem o sistema

### **4. Otimização de Performance** ⚡
- **Antes**: "Está lento, mas não sei por quê"
- **Depois**: "8.92 pkt/s, 2288 B/s" → Dados concretos para otimização

---

## 🎯 Resultado Final

Com este sistema de monitoramento, você tem:

✅ **Visibilidade completa** do que está sendo interceptado  
✅ **Logs estruturados** para debugging  
✅ **Métricas em tempo real** de performance  
✅ **Alertas automáticos** de problemas  
✅ **API simples** para integração  
✅ **Configuração flexível** via appsettings.json  

O sistema agora oferece **transparência total** sobre o processo de interceptação na porta 5050! 🎉