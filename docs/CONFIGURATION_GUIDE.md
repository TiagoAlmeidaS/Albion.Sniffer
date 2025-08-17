## Configuração unificada (App e Web)

Este guia documenta como configurar a publicação de eventos (fila) e bin-dumps tanto no `AlbionOnlineSniffer.App` quanto no `AlbionOnlineSniffer.Web`, usando a mesma convenção de chaves.

### RabbitMQ

- Use uma destas chaves (ordem de precedência):
  - `ConnectionStrings:RabbitMQ`
  - `RabbitMQ:ConnectionString`

- Exchange:
  - `RabbitMQ:Exchange`

Exemplo (appsettings.json):

```json
{
  "ConnectionStrings": {
    "RabbitMQ": "amqp://localhost"
  },
  "RabbitMQ": {
    "ConnectionString": "amqp://localhost",
    "Exchange": "albion.sniffer"
  }
}
```

O Web também aceitava anteriormente `Queue:ConnectionString` e `Queue:ExchangeName`. Mantemos compatibilidade de leitura, mas a configuração oficial é via `RabbitMQ`/`ConnectionStrings` conforme acima.

### BinDumps

- Chaves comuns (App e Web):
  - `BinDumps:Enabled` (bool, default: true)
  - `BinDumps:BasePath` (string, default: `ao-bin-dumps`)

### Publicação de eventos (App e Web)

- Ambos usam o `EventDispatcher` do Core e um bridge de fila para publicar eventos em tópicos no formato:
  - `albion.event.{tipoEvento}`
  - O sufixo `Event` é removido do nome do tipo (ex.: `NewCharacterEvent` → `albion.event.newcharacter`).

### Pipeline de captura e parsing

- App: `CapturePipeline` conecta `IPacketCaptureService` → `Protocol16Deserializer`.
- Web: `SnifferWebPipeline` faz o mesmo e também transmite para o SignalR (`SnifferHub`) e atualiza métricas.

### Como rodar apenas o Web

- O Web agora também publica eventos para a fila. Você pode executar somente o Web se quiser captura + parsing + SignalR + publicação de eventos.

### Resumo das responsabilidades

- Core: DI, offsets/indexes, desserialização, dispatch de eventos
- Capture: serviço de captura UDP (`IPacketCaptureService`)
- Queue: publishers (`IQueuePublisher`), bridge `EventToQueueBridge`
- App: pipeline de captura (`CapturePipeline`) + orquestração console
- Web: pipeline web (`SnifferWebPipeline`) + SignalR + API/Health/Métricas


