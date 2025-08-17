## Testes para o módulo Web

Este documento orienta a criação de testes unitários para o módulo Web.

### Áreas a testar

- SnifferWebPipeline
  - Liga `IPacketCaptureService.OnUdpPayloadCaptured` → desserializador e SignalR
  - Registra handler global no `EventDispatcher` para enviar eventos ao hub e métricas
  - Contabiliza métricas de pacotes e eventos, além de erros

- Registro de fila no Web
  - `DependencyProvider.AddQueueServices(services, configuration)` é chamado no Web para registrar `IQueuePublisher` e `EventToQueueBridge`
  - Pode ser validado por resolução de dependências em um `ServiceCollection` de teste

### Estratégia de teste

- Mockar `IPacketCaptureService` com um stub simples que expõe o evento e métodos `Start/Stop`
- Mockar `IHubContext<SnifferHub>` (SignalR) – verificar que chamadas de `SendAsync` são realizadas
- Usar `ServiceCollection` para montar o grafo de dependências mínimo, registrar o `SnifferWebPipeline` e disparar eventos/pacotes

### Exemplo de esqueleto

```csharp
// Projeto: AlbionOnlineSniffer.Tests
// Arquivo sugerido: Web/SnifferWebPipelineTests.cs

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Moq;
using AlbionOnlineSniffer.Web.Services;
using AlbionOnlineSniffer.Web.Hubs;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Core.Services;

public class SnifferWebPipelineTests
{
    [Fact]
    public async Task OnUdpPayloadCaptured_ShouldForwardToHub_AndDeserializer_AndMetrics()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(b => {}));
        services.AddSingleton<EventDispatcher>(sp => new EventDispatcher(sp.GetRequiredService<ILoggerFactory>().CreateLogger<EventDispatcher>()));
        services.AddSingleton<Protocol16Deserializer>(sp => {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<Protocol16Deserializer>();
            return new Protocol16Deserializer(new Albion.Network.ReceiverBuilder().Build(), logger);
        });
        var hubMock = new Mock<IHubContext<SnifferHub>>();
        services.AddSingleton(hubMock.Object);
        services.AddSingleton<EventStreamService>();
        services.AddSingleton<InboundMessageService>();
        services.AddSingleton<MetricsService>();

        // Stub de captura
        var captureMock = new Mock<IPacketCaptureService>();
        services.AddSingleton(captureMock.Object);

        services.AddSingleton<SnifferWebPipeline>();
        var sp = services.BuildServiceProvider();

        // resolve para executar wiring
        var pipeline = sp.GetRequiredService<SnifferWebPipeline>();

        // dispara evento de captura
        var payload = new byte[] {1,2,3};
        captureMock.Raise(c => c.OnUdpPayloadCaptured += null, payload);

        // asserts – verificar via mocks chamadas esperadas (ex: SendAsync)
        hubMock.Verify(h => h.Clients.All.SendAsync("udpPayload", It.IsAny<object?>(), default), Times.AtLeastOnce());
    }
}
```

Observação: adapte os mocks e asserts conforme sua política de testes (por exemplo, usar `It.Is<object>(o => ...)` para validar conteúdo).


