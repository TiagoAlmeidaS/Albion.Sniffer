# Plano de Implementação - Albion Sniffer Evolution

## 📋 Visão Geral

Este documento acompanha o progresso da evolução do Albion.Sniffer, transformando-o em um sistema modular, observável e plugável baseado no modelo do deatheye.

### Objetivo Principal
Evoluir o Albion.Sniffer para:
- Sistema headless modular
- Observável e testável
- Plugável com Radar/overlay web
- Compatível com estrutura deatheye (Settings/, ITEMS/, ao-bin-dumps/, rabbitmq.json)

### Timeline Estimada
- **Total**: ~17-19 dias de engenharia
- **Início**: $(date)
- **Previsão de Conclusão**: $(date + 19 dias)

---

## 🚀 Fases de Implementação

### ✅ Fase 0 - Baseline & Hardening (1 dia)
**Status**: ✅ Concluída  
**Início**: 2024-12-28
**Conclusão**: 2024-12-28

#### Objetivos
- Estabilizar o que já existe
- Travar interfaces públicas
- Preparar terreno para mudanças

#### Tarefas
- [x] Congelar interfaces públicas atuais
- [x] Marcar APIs obsoletas com [Obsolete]
- [x] Habilitar validação de Options com ValidateOnStart()
- [x] Adicionar DataAnnotations
- [x] Configurar dotnet format + lint
- [x] Criar EditorConfig compartilhado

#### Critérios de Aceite
- ✅ App falha explicitamente se config estiver inválida
- ✅ Build + testes "verdes" localmente e no CI
- ✅ Linting aplicado em todo código

#### Artefatos Criados
- `/workspace/.editorconfig` - Configuração de estilo de código
- `/workspace/.globalconfig` - Configuração de análise de código
- `/workspace/.config/dotnet-tools.json` - Ferramentas dotnet locais
- `/workspace/src/AlbionOnlineSniffer.Options/` - Novo projeto de configurações
- `/workspace/format-code.sh` - Script de formatação e linting
- Estrutura completa de Options com validação

---

### ✅ Fase 1 - Profiles & Personalizações (2 dias)
**Status**: ✅ Concluída
**Início**: 2024-12-28
**Conclusão**: 2024-12-28

#### Objetivos
- Transformar injeções do projeto base em Profiles configuráveis
- Remover acoplamento do core

#### Tarefas
- [x] Criar ProfileOptions
- [x] Incluir em SnifferOptions
- [x] Implementar resolução de profile (CLI > ENV > config)
- [x] Mapear toggles/cores/tiers do deatheye
- [x] Criar mínimo 3 perfis (default, ZvZ, Gank)

#### Critérios de Aceite
- ✅ `dotnet run --profile ZvZ` aplica configurações distintas
- ✅ Sem mudanças em código de parser ao trocar profile

#### Artefatos Criados
- `ProfileOptions.cs` - Estrutura de perfis configuráveis
- `ProfileManager.cs` - Gerenciador de perfis com eventos
- `TierPalettes.cs` - Sistema de paletas de cores (classic, vibrant, minimal)
- `IEventEnricher.cs` - Sistema de enrichers configuráveis
- `DeatheyeProfileMapper.cs` - Mapeador de configurações deatheye (corrigido typo)
- `appsettings.Development.json` - Exemplos de perfis configurados

---

### 🟡 Fase 2 - Providers Plugáveis (3 dias)
**Status**: Em progresso (básico pronto; melhorias em PR dedicado)
**Início**: 2024-12-28
**Conclusão**: a definir

#### Objetivos
- Remover acoplamento a arquivos locais
- Criar providers de metadados e dumps

#### Tarefas
- [x] Interface IBinDumpProvider
- [x] Interface IItemMetadataProvider
- [x] Implementar FileSystemProvider
- [x] Implementar EmbeddedResourceProvider
- [ ] Implementar HttpCachedProvider (PR dedicado)
- [x] Seleção por Options
- [x] Versionamento de dumps

#### Critérios de Aceite
- ✅ Trocar provider sem recompilar
- ✅ Métricas/health em caso de falha

#### Artefatos Criados
- `IBinDumpProvider.cs` - Interface para providers de dumps binários
- `IItemMetadataProvider.cs` - Interface para providers de metadados
- `FileSystemBinDumpProvider.cs` - Provider para arquivos locais
- `EmbeddedResourceProvider.cs` - Provider para recursos embarcados
- `ProviderFactory.cs` - Factory para seleção de providers

---

### 📅 Fase 3 - Contratos de Eventos V1 (2 dias)
**Status**: ⏳ Pendente

#### Objetivos
- Definir payloads estáveis e versionados
- Garantir compatibilidade forward/backward

#### Tarefas
- [ ] Namespace Albion.Events.V1.*
- [ ] Criar eventos base (PlayerSpotted, DungeonFound, etc)
- [ ] Serialização MessagePack com fallback JSON
- [ ] Tópicos com sufixo de versão
- [ ] Golden tests (snapshot)

#### Critérios de Aceite
- Consumidor dummy valida eventos V1
- Ambos formatos funcionais

---

### 📅 Fase 4 - Pipeline Assíncrono (2 dias)
**Status**: ⏳ Pendente

#### Objetivos
- Evitar quedas com oscilação de fila
- Melhorar latência

#### Tarefas
- [ ] Implementar Channel pipeline
- [ ] IEventEnricher configurável
- [ ] Polly retry policies
- [ ] Circuit breaker

#### Critérios de Aceite
- Simulação de pressão não trava captura
- Contadores de drop expostos

---

### 📅 Fase 5 - Observabilidade (2 dias)
**Status**: ⏳ Pendente

#### Objetivos
- Visibilidade total do fluxo

#### Tarefas
- [ ] OpenTelemetry metrics
- [ ] HealthChecks
- [ ] Logs estruturados (Serilog)
- [ ] EventId de correlação

#### Critérios de Aceite
- /health reporta status por dependência
- Métricas visíveis no consumidor

---

### 📅 Fase 6 - Testes (2 dias)
**Status**: ⏳ Pendente

#### Objetivos
- Cobertura do fluxo crítico

#### Tarefas
- [ ] Unit tests
- [ ] Contract tests
- [ ] Integration tests
- [ ] Dados sintéticos

#### Critérios de Aceite
- Cobertura de happy path
- Testes de perfil alternado
- Testes de fila instável

---

### 📅 Fase 7 - Consumidor de Referência (2 dias)
**Status**: ⏳ Pendente

#### Objetivos
- Provar separação Sniffer ↔ UI

#### Tarefas
- [ ] Projeto sample-consumer
- [ ] Consumir de Rabbit/Redis
- [ ] Renderizar payloads
- [ ] Overlay web opcional

#### Critérios de Aceite
- Alternar profiles sem mudanças no consumidor

---

### 📅 Fase 8 - Deployability (1 dia)
**Status**: ⏳ Pendente

#### Objetivos
- Reprodutibilidade e distribuição

#### Tarefas
- [ ] Dockerfile multi-stage
- [ ] Single-file publishing
- [ ] Matrix SO/Drivers

#### Critérios de Aceite
- Docker run funcional
- Binários para win/linux

---

### 📅 Fase 9 - Documentação (1 dia)
**Status**: ⏳ Pendente

#### Objetivos
- Repositório pronto para contribuir

#### Tarefas
- [ ] Contracts V1 docs
- [ ] Providers docs
- [ ] Profiles docs
- [ ] Observability docs
- [ ] 2-PC guide

#### Critérios de Aceite
- Onboarding < 30min

---

## 📊 Métricas de Progresso

| Fase | Status | Progresso | Tempo Estimado | Tempo Real |
|------|--------|-----------|----------------|------------|
| 0 | ✅ Concluída | 100% | 1 dia | < 1 dia |
| 1 | ✅ Concluída | 100% | 2 dias | < 1 dia |
| 2 | 🟡 Em progresso | 70% | 3 dias | - |
| 3 | ⏳ Pendente | 0% | 2 dias | - |
| 4 | ⏳ Pendente | 0% | 2 dias | - |
| 5 | ⏳ Pendente | 0% | 2 dias | - |
| 6 | ⏳ Pendente | 0% | 2 dias | - |
| 7 | ⏳ Pendente | 0% | 2 dias | - |
| 8 | ⏳ Pendente | 0% | 1 dia | - |
| 9 | ⏳ Pendente | 0% | 1 dia | - |

**Total Geral**: 27% completo (2.7 de 10 fases)

---

## 📝 Notas de Implementação

### Progresso Acelerado (2024-12-28)
- **Fases 0 e 1 concluídas em menos de 1 dia**
- Implementação eficiente devido à arquitetura bem planejada
- Reutilização de padrões estabelecidos no projeto base

### Fase 2 - Providers Plugáveis
- Sistema de providers configurável (FileSystem, Embedded)
- HTTP/cache/ETag/logs serão implementados em PR dedicado

### Fase 1 - Profiles & Personalizações  
- Sistema de profiles completo com 3 paletas de cores
- Enrichers modulares e compostos (serão ligados no pipeline – PR F4)
- Mapeamento automático de configurações deatheye

### Fase 0 - Baseline
- Estrutura de Options com validação completa
- EditorConfig e análise de código configurados
- Interfaces obsoletas marcadas para remoção futura

---

## 🔗 Referências

- [Repositório deatheye](https://github.com/TiagoAlmeidaS/albion-radar-deatheye-2pc)
- [Albion.Sniffer Original](README.md)
- [Arquitetura Alvo](architecture_plan.md)