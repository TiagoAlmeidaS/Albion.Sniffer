### Implementação e Expansão de Eventos de Pesca

Este documento consolida o estado atual do suporte à pesca no projeto e define um plano de expansão compatível com o ecossistema existente (`indexes.json`, `offsets.json`, modelos, handlers e eventos v1). O objetivo é evoluir do suporte básico a zonas de pesca para um conjunto completo de eventos relacionados ao ciclo de pesca (início, mordida, minigame e término), mantendo compatibilidade com os loaders atuais.

#### 1) Estado atual (confirmado no repositório)

- **Index mapeado**: existe a entrada `NewFishingZoneObject` em `indexes.json`:

```18:20:src/AlbionOnlineSniffer.Core/Data/jsons/indexes.json
    "NewDungeonExit": 319,
    "NewFishingZoneObject": 355,
    "ChangeFlaggingFinished": 359,
```

- **Offsets mapeados**: `offsets.json` contém `NewFishingZoneObject` com offsets mínimos. O modelo usa os dois primeiros índices para `Id` e `PositionBytes`.

- **Handler**: há um handler dedicado que consome o índice de `NewFishingZoneObject` e despacha evento V1 com posição decriptografada:

```16:21:src/AlbionOnlineSniffer.Core/Handlers/NewFishingZoneEventHandler.cs
        public NewFishingZoneEventHandler(FishNodesHandler fishZoneHandler, EventDispatcher eventDispatcher, LocationService locationService) : base(PacketIndexesLoader.GlobalPacketIndexes?.NewFishingZoneObject ?? 0)
        {
            this.fishZoneHandler = fishZoneHandler;
            this.eventDispatcher = eventDispatcher;
            this.locationService = locationService;
        }
```

- **Modelo do evento**: consome `PacketOffsets.NewFishingZoneObject` e lê `Id` e `PositionBytes` (índices 0 e 1):

```13:21:src/AlbionOnlineSniffer.Core/Models/Events/NewFishingZoneEvent.cs
        public NewFishingZoneEvent(Dictionary<byte, object> parameters, PacketOffsets packetOffsets) : base(parameters)
        {
            offsets = packetOffsets?.NewFishingZoneObject ?? new byte[] { 0, 1 };
            
            Id = Convert.ToInt32(parameters[offsets[0]]);

            if (parameters.ContainsKey(offsets[1]) && parameters[offsets[1]] is byte[] positionBytes)
            {
                PositionBytes = positionBytes;
            }
```

Conclusão do diagnóstico: o projeto já mapeia e processa a criação de zonas de pesca, mas não há ainda eventos para o ciclo completo de pesca (lançamento, mordida, updates do minigame e término). Os loaders existentes exigem estruturas simples (flat) e são incompatíveis com um nó aninhado como `"FishingEvents": { ... }` dentro de `offsets.json`.

#### 2) Diretrizes de compatibilidade (loaders atuais)

- `PacketIndexesLoader` desserializa diretamente `indexes.json` em `PacketIndexes` (propriedades públicas). Para novos eventos, é necessário adicionar novas propriedades em `PacketIndexes` e entradas correspondentes no JSON.
- `PacketOffsetsLoader` espera um `Dictionary<string, int[]>` e converte cada array em `byte[]` para uma propriedade homônima em `PacketOffsets`. Portanto, os novos eventos devem ser adicionados como chaves de topo (flat) com arrays de inteiros, não como objetos aninhados.

Implicação: mantenha o padrão existente. Não introduza nós como `FishingEvents`. Em vez disso, adicione entradas raiz para cada novo evento de pesca.

#### 3) Proposta de expansão (flat, compatível)

Adicionar novos eventos relacionados à pesca, todos como chaves de topo em `indexes.json` e `offsets.json`, com os seguintes nomes e papéis:

- `StartFishing` (client request): início do lançamento
- `FishingBiteEvent` (server event): mordida/notificação
- `FishingMiniGameUpdate` (server event): atualizações da barra/minigame
- `FishingFinish` (server event): resultado da captura

Também considerar enriquecer `NewFishingZoneObject` caso surjam campos adicionais úteis (tier/tipo de água/tamanho), preservando retrocompatibilidade com os dois primeiros offsets já usados.

Sugestões de índices (placeholders a validar in-game):

- `StartFishing`: 352–360 (escolher um livre, ex.: 352)
- `FishingBiteEvent`: 361–380 (ex.: 362)
- `FishingMiniGameUpdate`: 381–386 (ex.: 385)
- `FishingFinish`: 246–260 (ex.: 246)

Observação: os valores acima são indicativos e precisam ser confirmados por captura real. O projeto já usa `355` para `NewFishingZoneObject` (confirmado).

#### 4) offsets.json (flat) – formato sugerido

Manter arrays de índices de parâmetros (conforme já adotado). Cada posição refere-se à chave do dicionário de parâmetros recebido do Photon, respeitando o estilo atual (ex.: `[0, 1]` para `Id` e `PositionBytes`). Exemplos iniciais para instrumentação (placeholders):

- `NewFishingZoneObject`: manter ao menos `[0, 1]` (Id, PositionBytes). Campos extras, se identificados, podem seguir como `[2, 3, ...]`.
- `StartFishing`: `[0, 1, 2, 3]` (ex.: `RodId`, `BaitId`, `TargetPosX`, `TargetPosY`)
- `FishingBiteEvent`: `[0, 1, 2]` (ex.: `FishId`, `Difficulty`, `BiteTime`)
- `FishingMiniGameUpdate`: `[0, 1, 2]` (ex.: `BobPosition`, `BarSpeed`, `Direction`)
- `FishingFinish`: `[0, 1, 2, 3]` (ex.: `Result`, `ItemId`, `Quantity`, `Rarity`)

Esses arrays devem ser calibrados com logs in-game. A interpretação dos tipos é feita nas classes de evento (C#), não no JSON.

#### 5) Alterações de código necessárias

1. `src/AlbionOnlineSniffer.Core/Models/ResponseObj/PacketIndexes.cs`
   - Adicionar propriedades `int StartFishing`, `int FishingBiteEvent`, `int FishingMiniGameUpdate`, `int FishingFinish`.

2. `src/AlbionOnlineSniffer.Core/Data/jsons/indexes.json`
   - Adicionar entradas inteiras para as novas propriedades acima com os opcodes validados (placeholder até validação).

3. `src/AlbionOnlineSniffer.Core/Models/ResponseObj/PacketOffsets.cs`
   - Adicionar propriedades `byte[] StartFishing`, `byte[] FishingBiteEvent`, `byte[] FishingMiniGameUpdate`, `byte[] FishingFinish`.

4. `src/AlbionOnlineSniffer.Core/Data/jsons/offsets.json`
   - Adicionar arrays de índices conforme seção 4 (placeholders). Ajustar após captura real.

5. Modelos de evento (ex.: `StartFishingEvent`, `FishingBiteEvent`, `FishingMiniGameUpdateEvent`, `FishingFinishEvent`)
   - Padrão: construtor recebe `Dictionary<byte, object> parameters, PacketOffsets offsets` e lê posições conforme o array, convertendo tipos (int/long/float/byte[]). Onde houver posição codificada de posição, utilizar `LocationService` para decifrar coordenadas.

6. Handlers (`EventPacketHandler<T>`) para cada novo evento
   - O construtor deve usar `PacketIndexesLoader.GlobalPacketIndexes?.<NovoEvento> ?? 0`.
   - Em `OnActionAsync`, montar e despachar eventos V1 adequados.

7. Eventos V1 (em `src/Albion.Events.V1/`)
   - Adicionar tipos mínimos para telemetry/saída: `FishingStartedV1`, `FishingBiteV1`, `FishingMiniGameUpdatedV1`, `FishingFinishedV1`.

#### 6) Estratégia de validação ("verificar se contém de fato")

Passos para confirmar e ajustar os placeholders com dados reais:

1. Habilitar logs de captura durante sessões de pesca: lançar linha, aguardar mordida, completar/falhar minigame.
2. Registrar opcodes observados no pipeline (output de debug) e mapear para os novos nomes.
3. Para cada evento:
   - Confirmar a cardinalidade de parâmetros (tamanho do dicionário) e seus tipos reais.
   - Ajustar `indexes.json` (opcodes) e `offsets.json` (arrays de índices) conforme necessário.
4. Validar decodificação de posição usando `LocationService` quando aplicável.
5. Confirmar que os eventos V1 estão sendo emitidos com dados coerentes (ex.: `Tier`, `Difficulty`, `Result`).

Critérios de aceite:

- Criação de zona de pesca continua funcionando (retrocompatibilidade mantida).
- Logs mostram recepção e parsing corretos de `StartFishing`, `FishingBiteEvent`, `FishingMiniGameUpdate`, `FishingFinish` durante um ciclo completo.
- Eventos V1 correspondentes são emitidos com campos não nulos/coerentes.

#### 7) Riscos e mitigação

- Mudanças de protocolo (patches do jogo) podem invalidar offsets/opcodes: mitigar com uma rotina de detecção de falhas de parsing e feature flag para desativar parsing de pesca.
- Bans: manter sniffing passivo e usar contas alternativas para testes.

#### 8) Checklist de implementação

- [ ] Adicionar propriedades em `PacketIndexes` e `PacketOffsets` para novos eventos.
- [ ] Preencher `indexes.json` com opcodes capturados (inicialmente placeholders).
- [ ] Preencher `offsets.json` com arrays de índices (inicialmente placeholders).
- [ ] Criar modelos `StartFishingEvent`, `FishingBiteEvent`, `FishingMiniGameUpdateEvent`, `FishingFinishEvent`.
- [ ] Criar handlers correspondentes.
- [ ] Criar eventos V1 e integrar no `EventDispatcher`.
- [ ] Capturar sessão de pesca end-to-end e ajustar offsets/opcodes.
- [ ] Verificar retrocompatibilidade de `NewFishingZoneObject` e eventos já existentes.

Notas finais:

- O padrão flat em `offsets.json` e `indexes.json` é obrigatório para compatibilidade com os loaders atuais. Evite estruturas aninhadas.
- Os nomes propostos dos eventos são coerentes com a convenção já usada (PascalCase) e simplificam a leitura/manutenção.


