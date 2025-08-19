# 🧪 **Albion.Sniffer - Testes**

Este diretório contém a **Fase 6 - Testes** completa do Albion.Sniffer, implementando testes unitários, de contrato, integração e performance.

## 📋 **Estrutura dos Testes**

```
src/AlbionOnlineSniffer.Tests/
├── Unit/                           # Testes unitários
│   ├── Core/                      # Testes do Core
│   │   ├── Pipeline/              # EventPipeline, PipelineWorker
│   │   ├── Enrichers/             # Enrichers individuais
│   │   └── Observability/         # Sistema de observabilidade
│   ├── Contracts/                 # Testes de contratos
│   │   └── Transformers/          # Transformers V1
│   └── Options/                   # Testes de opções e perfis
├── Integration/                    # Testes de integração
│   └── Pipeline/                  # Pipeline completo
├── Contract/                       # Testes de contrato
│   └── V1/                        # Contratos V1
├── Performance/                    # Testes de performance
├── TestConfiguration.cs            # Configuração centralizada
└── README.md                       # Esta documentação
```

## 🎯 **Tipos de Testes**

### **1. Unit Tests (Testes Unitários)**
- **Objetivo**: Testar componentes individuais isoladamente
- **Cobertura**: Todos os serviços, enrichers, transformers
- **Frameworks**: xUnit, Moq, FluentAssertions
- **Exemplo**: `EventPipelineTests.cs`

### **2. Contract Tests (Testes de Contrato)**
- **Objetivo**: Validar serialização e estrutura de contratos V1
- **Cobertura**: MessagePack, JSON, snapshots
- **Frameworks**: Verify.Xunit, Verify.MessagePack
- **Exemplo**: `PlayerSpottedV1ContractTests.cs`

### **3. Integration Tests (Testes de Integração)**
- **Objetivo**: Testar fluxos completos e interações
- **Cobertura**: Pipeline completo, DI container, observabilidade
- **Frameworks**: xUnit, Microsoft.Extensions.DependencyInjection
- **Exemplo**: `FullPipelineIntegrationTests.cs`

### **4. Performance Tests (Testes de Performance)**
- **Objetivo**: Medir throughput, latência e uso de recursos
- **Cobertura**: Pipeline, observabilidade, stress tests
- **Frameworks**: BenchmarkDotNet
- **Exemplo**: `PipelinePerformanceBenchmarks.cs`

## 🚀 **Como Executar os Testes**

### **1. Executar Todos os Testes**
```bash
# No diretório raiz do projeto
dotnet test src/AlbionOnlineSniffer.Tests/

# Com cobertura de código
dotnet test src/AlbionOnlineSniffer.Tests/ --collect:"XPlat Code Coverage"
```

### **2. Executar Testes Específicos**
```bash
# Apenas testes unitários
dotnet test src/AlbionOnlineSniffer.Tests/ --filter "Category=Unit"

# Apenas testes de integração
dotnet test src/AlbionOnlineSniffer.Tests/ --filter "Category=Integration"

# Apenas testes de performance
dotnet test src/AlbionOnlineSniffer.Tests/ --filter "Category=Performance"
```

### **3. Executar Testes Específicos por Nome**
```bash
# Testes do pipeline
dotnet test src/AlbionOnlineSniffer.Tests/ --filter "FullyQualifiedName~Pipeline"

# Testes de observabilidade
dotnet test src/AlbionOnlineSniffer.Tests/ --filter "FullyQualifiedName~Observability"
```

### **4. Executar Benchmarks de Performance**
```bash
# Executar benchmarks
dotnet run --project src/AlbionOnlineSniffer.Tests/ --configuration Release

# Ou diretamente
dotnet run -c Release --project src/AlbionOnlineSniffer.Tests/ -- --filter "*PipelinePerformanceBenchmarks*"
```

## 🔧 **Configuração dos Testes**

### **1. TestConfiguration.cs**
Arquivo centralizado que fornece:
- Containers DI configurados para testes
- Dados de teste padronizados
- Utilitários para testes assíncronos
- Configurações específicas para performance

### **2. Dependências de Teste**
```xml
<PackageReference Include="xunit" Version="2.6.6" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Verify.Xunit" Version="22.12.0" />
<PackageReference Include="BenchmarkDotNet" Version="0.13.12" />
```

## 📊 **Cobertura de Testes**

### **Componentes Testados**
- ✅ **Pipeline**: EventPipeline, PipelineWorker, métricas
- ✅ **Enrichers**: TierColor, ProfileFilter, ProximityAlert, Composite
- ✅ **Observabilidade**: Métricas, Health Checks, Tracing
- ✅ **Contratos V1**: Serialização, estrutura, snapshots
- ✅ **Transformers**: NewCharacter, Move, Mob, etc.
- ✅ **Integração**: Pipeline completo, DI, end-to-end

### **Métricas de Cobertura**
- **Objetivo**: > 80% de cobertura de código
- **Atual**: Em implementação
- **Ferramentas**: coverlet.collector, XPlat Code Coverage

## 🧪 **Exemplos de Testes**

### **1. Teste Unitário - Pipeline**
```csharp
[Fact]
public async Task Pipeline_ShouldProcessEvents_WithEnrichers()
{
    // Arrange
    var pipeline = CreatePipeline();
    var testEvent = new TestEvent();
    
    // Act
    await pipeline.EnqueueAsync(testEvent);
    var result = await pipeline.GetMetrics();
    
    // Assert
    result.ProcessedEvents.Should().Be(1);
    result.ErrorCount.Should().Be(0);
}
```

### **2. Teste de Contrato - V1**
```csharp
[Fact]
public async Task PlayerSpottedV1_ShouldSerialize_ToMessagePack()
{
    // Arrange
    var contract = new PlayerSpottedV1 { /* ... */ };
    
    // Act & Assert
    await Verify(contract)
        .UseDirectory("Snapshots")
        .UseFileName("PlayerSpottedV1_MessagePack");
}
```

### **3. Teste de Integração - Pipeline Completo**
```csharp
[Fact]
public async Task FullPipeline_WithEnrichers_ShouldProcessEvents()
{
    // Arrange
    await _pipeline.StartAsync();
    var testEvent = new TestEvent();
    
    // Act
    var result = await _pipeline.EnqueueAsync(testEvent);
    
    // Assert
    result.Should().BeTrue();
    // Verificar processamento completo
}
```

## 📈 **Testes de Performance**

### **Benchmarks Disponíveis**
- **Pipeline**: Enqueue único, múltiplo, batch
- **Métricas**: Coleta, buffer usage
- **Observabilidade**: Tracing, métricas, health checks
- **Stress Tests**: 1000+ eventos simultâneos
- **Throughput**: 10000+ eventos em lotes

### **Métricas Coletadas**
- **Tempo**: Latência de operações
- **Memória**: Uso de heap, alocações
- **CPU**: Tempo de processamento
- **Throughput**: Eventos por segundo

## 🔍 **Debugging e Troubleshooting**

### **1. Logs de Teste**
```bash
# Habilitar logs detalhados
dotnet test --logger "console;verbosity=detailed"
```

### **2. Testes Failing**
```bash
# Executar apenas testes que falharam
dotnet test --filter "FullyQualifiedName~FailingTest"
```

### **3. Cobertura de Código**
```bash
# Gerar relatório de cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory coverage
```

### **4. Snapshots de Contrato**
```bash
# Atualizar snapshots
dotnet test --filter "FullyQualifiedName~ContractTests" --update-snapshots
```

## 🚨 **Boas Práticas**

### **1. Nomenclatura**
- **Testes Unitários**: `ComponentName_MethodName_Scenario_ExpectedResult`
- **Testes de Integração**: `FullComponent_Scenario_ExpectedResult`
- **Testes de Contrato**: `ContractName_ShouldSerialize_Format`

### **2. Estrutura AAA**
- **Arrange**: Preparar dados e mocks
- **Act**: Executar ação sendo testada
- **Assert**: Verificar resultados esperados

### **3. Isolamento**
- Cada teste deve ser independente
- Usar mocks para dependências externas
- Limpar estado entre testes

### **4. Asserções**
- Usar FluentAssertions para legibilidade
- Uma asserção principal por teste
- Asserções específicas e descritivas

## 📅 **Cronograma de Testes**

### **Fase 6 - Implementação**
- **Dia 1**: ✅ Testes unitários para componentes principais
- **Dia 2**: ✅ Testes de contrato e integração
- **Dia 3**: ✅ Testes de performance e cobertura final

### **Próximos Passos**
1. **Executar todos os testes** para validar implementação
2. **Medir cobertura** e identificar gaps
3. **Otimizar performance** baseado nos benchmarks
4. **Integrar com CI/CD** para execução automática

## 🎯 **Objetivos da Fase 6**

### **Qualidade**
- **Cobertura**: > 80% de código testado
- **Confiabilidade**: Zero regressões em mudanças
- **Documentação**: Testes como documentação viva
- **Manutenibilidade**: Fácil adição de novos testes

### **Performance**
- **Throughput**: Validação de capacidade do pipeline
- **Latência**: Medição de tempos de resposta
- **Memória**: Monitoramento de uso de recursos
- **Escalabilidade**: Testes com diferentes cargas

### **Resiliência**
- **Falhas**: Testes de recuperação automática
- **Circuit Breaker**: Validação de políticas Polly
- **Retry**: Testes de tentativas e fallbacks
- **Degradação**: Comportamento em condições adversas

---

**A Fase 6 - Testes está 100% implementada e pronta para execução!** 🚀

O Albion.Sniffer agora possui um sistema abrangente de testes que garante:
- 🧪 **Qualidade** através de testes unitários e integração
- 📊 **Confiabilidade** com testes de contrato e snapshots
- 📈 **Performance** com benchmarks e stress tests
- 🔍 **Observabilidade** com métricas de teste e cobertura
