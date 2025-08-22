# AlbionOnlineSniffer.Tests

## 📁 Estrutura de Testes

```
AlbionOnlineSniffer.Tests/
├── Core/                      # Testes do Core (parser, eventos, enrichers)
│   ├── PhotonParserTests.cs
│   ├── ContractsV1_SnapshotTests.cs
│   └── EventEnricher_*_Tests.cs
├── Capture/                   # Testes do Capture (SharpPcap/libpcap)
│   ├── PacketCaptureTests.cs
│   └── PcapFileReaderTests.cs
├── Queue/                     # Testes do Queue (Rabbit/Redis)
│   ├── RabbitPublisher_IntegrationTests.cs
│   ├── RedisPublisher_IntegrationTests.cs
│   └── Publisher_RetryPolicy_Tests.cs
├── App/                       # Testes do App (host, configuração)
│   ├── Host_Startup_SmokeTests.cs
│   └── Profile_Binding_CliEnv_Tests.cs
├── E2E/                       # Testes End-to-End
│   └── Pipeline_EndToEnd_Tests.cs
├── Common/                    # Utilitários compartilhados
│   ├── IClock.cs             # Abstração de tempo
│   ├── IIdGenerator.cs       # Gerador de IDs determinístico
│   ├── Builders/             # Builders para dados de teste
│   │   ├── DomainEventBuilder.cs
│   │   └── PhotonPacketBuilder.cs
│   └── Fakes/                # Implementações fake
│       ├── FakePacketCaptureService.cs
│       └── InMemoryPublisher.cs
└── Snapshots/                 # Snapshots para testes de contrato
    └── ContractsV1/
        ├── *.verified.json
        └── *.verified.msgpack
```

## 🧪 Tipos de Teste

### Unit Tests (Puros)
- **Objetivo**: Testar lógica isolada sem dependências externas
- **Exemplos**: Parser Photon, Enrichers, Serialização
- **Categoria**: `[Trait("Category", "Unit")]`

### Integration Tests
- **Objetivo**: Testar integração com serviços externos
- **Exemplos**: RabbitMQ, Redis, Database
- **Categoria**: `[Trait("Category", "Integration")]`
- **Requisitos**: Docker/Testcontainers

### E2E Tests
- **Objetivo**: Validar pipeline completo
- **Exemplos**: Captura → Parser → Enrich → Publish
- **Categoria**: `[Trait("Category", "E2E")]`

### Contract/Snapshot Tests
- **Objetivo**: Garantir compatibilidade de contratos
- **Exemplos**: Serialização JSON/MessagePack estável
- **Ferramenta**: Verify.Xunit

## 🚀 Executando Testes

### Linux/Mac
```bash
# Todos os testes
./run-tests.sh

# Apenas unitários
./run-tests.sh --unit

# Com cobertura
./run-tests.sh --coverage

# Em modo watch
./run-tests.sh --watch

# Filtro específico
./run-tests.sh --filter "FullyQualifiedName~PhotonParser"
```

### Windows
```powershell
# Todos os testes
.\run-tests.ps1

# Apenas unitários
.\run-tests.ps1 -Unit

# Com cobertura
.\run-tests.ps1 -Coverage

# Em modo watch
.\run-tests.ps1 -Watch

# Filtro específico
.\run-tests.ps1 -Filter "FullyQualifiedName~PhotonParser"
```

### Direto com dotnet
```bash
# Testes unitários
dotnet test --filter "Category!=Integration&Category!=E2E"

# Testes de integração
dotnet test --filter "Category=Integration"

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Em modo watch
dotnet watch test
```

## 📊 Cobertura de Código

Meta de cobertura:
- **Core**: 80%+ (lógica crítica)
- **Queue**: 80%+ (publishers)
- **Capture**: 60%+ (limitado por I/O)
- **App**: 50%+ (configuração/wiring)

Gerar relatório:
```bash
# Linux/Mac
./run-tests.sh --coverage

# Windows
.\run-tests.ps1 -Coverage

# Relatório será gerado em ./CoverageReport/index.html
```

## 🔧 Padrões e Convenções

### Naming
- Testes: `MethodName_Scenario_ExpectedBehavior`
- Fixtures: `*Fixture` ou `*TestBase`
- Builders: `*Builder`
- Fakes: `Fake*` ou `*Fake`

### Assertions
```csharp
// Use FluentAssertions para legibilidade
result.Should().NotBeNull();
result.Name.Should().Be("Expected");
result.Items.Should().HaveCount(3);

// Evite Assert clássico
Assert.NotNull(result); // ❌
Assert.Equal("Expected", result.Name); // ❌
```

### Determinismo
```csharp
// ✅ Use abstrações injetáveis
public MyService(IClock clock, IIdGenerator idGen) { }

// ❌ Evite dependências diretas
var now = DateTime.UtcNow; // Não determinístico
var id = Guid.NewGuid(); // Não determinístico
```

### Fakes vs Mocks
```csharp
// ✅ Prefira Fakes explícitos
var capture = new FakePacketCaptureService();
capture.EnqueuePacket(packet);

// ⚠️ Use Mocks apenas quando necessário
var mock = new Mock<IService>();
mock.Setup(x => x.Method()).Returns(value);
```

## 🐛 Troubleshooting

### Testes de integração falhando
1. Verifique se Docker está rodando
2. Confirme que as portas não estão em uso
3. Aumente timeouts se necessário

### Snapshots falhando
1. Revise mudanças nos contratos
2. Atualize snapshots se mudança for intencional:
   ```bash
   dotnet test --filter "SnapshotTests"
   # Aceite mudanças no Verify diff tool
   ```

### Cobertura baixa
1. Identifique código não coberto:
   ```bash
   ./run-tests.sh --coverage
   # Abra ./CoverageReport/index.html
   ```
2. Adicione testes para cenários faltantes
3. Considere excluir código não testável

## 📚 Recursos

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Verify](https://github.com/VerifyTests/Verify)
- [Testcontainers](https://dotnet.testcontainers.org/)
- [Bogus](https://github.com/bchavez/Bogus)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)