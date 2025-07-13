# Planejamento da Arquitetura do Sniffer de Rede em C# (.NET 8)

## 1. Introdução

Este documento detalha o planejamento da arquitetura para o desenvolvimento de um sniffer de rede em C# utilizando o .NET 8, com foco na captura de pacotes do jogo Albion Online e na publicação desses dados em uma fila de mensagens. A inspiração para este projeto vem do repositório `FashionFlora/Albion-Online-Radar-QRadar` (Node.js), que demonstrou a viabilidade de interceptar e processar pacotes do protocolo Photon.

O objetivo principal é criar um esqueleto robusto e modular que possa ser estendido para diversas finalidades, como análise de dados do jogo, monitoramento de atividades e integração com outros sistemas. A arquitetura proposta visa desacoplar as responsabilidades de captura, parsing e publicação, garantindo flexibilidade e escalabilidade.

## 2. Componentes da Arquitetura

A arquitetura será dividida em módulos lógicos, cada um com uma responsabilidade bem definida:

### 2.1. Módulo de Captura de Pacotes (Packet Capture Module)

Este módulo será responsável por interceptar o tráfego de rede na interface selecionada, filtrando os pacotes relevantes para o Albion Online. A escolha da biblioteca para captura de pacotes é crucial para o desempenho e a compatibilidade com diferentes sistemas operacionais.

**Responsabilidades:**
*   Identificar e selecionar a interface de rede.
*   Capturar pacotes de rede brutos.
*   Aplicar filtros (e.g., porta UDP 5056) para reduzir o volume de dados processados.
*   Encaminhar os pacotes capturados para o Módulo de Parsing.

**Tecnologias Sugeridas:**
*   **Pcap.Net:** Uma biblioteca .NET popular e madura para captura e injeção de pacotes, que é um wrapper para a biblioteca WinPcap/Npcap no Windows e libpcap no Linux. É uma escolha natural dado o contexto do projeto de referência.

### 2.2. Módulo de Parsing de Pacotes Photon (Photon Packet Parsing Module)

Este é o coração do sniffer, responsável por decodificar os payloads dos pacotes UDP que contêm dados do protocolo Photon. A complexidade reside na estrutura binária do protocolo Photon, que exige um parser customizado.

**Responsabilidades:**
*   Receber payloads UDP do Módulo de Captura.
*   Identificar a estrutura do protocolo Photon dentro do payload.
*   Decodificar os dados binários em estruturas de dados legíveis (e.g., objetos C#).
*   Disparar eventos ou retornar objetos que representem os eventos, requisições ou respostas do Photon.

**Tecnologias Sugeridas:**
*   **Implementação Customizada em C#:** Dada a natureza específica do protocolo Photon do Albion Online, será necessário desenvolver classes e métodos para interpretar os offsets e tipos de dados dentro dos pacotes. A análise do projeto de referência (`PhotonPacketParser.js`) será fundamental para guiar essa implementação.

### 2.3. Módulo de Publicação para Fila (Queue Publisher Module)

Este módulo será encarregado de enviar os dados parseados do Photon para uma fila de mensagens, garantindo o desacoplamento entre o sniffer e os consumidores dos dados. Isso permite que múltiplos serviços consumam os dados de forma assíncrona e escalável.

**Responsabilidades:**
*   Receber os eventos/dados parseados do Módulo de Parsing.
*   Serializar os dados para um formato adequado para a fila (e.g., JSON).
*   Conectar-se ao serviço de fila de mensagens.
*   Publicar as mensagens na fila designada.

**Tecnologias Sugeridas:**
*   **RabbitMQ (com cliente .NET RabbitMQ.Client):** Uma escolha robusta para sistemas de mensagens, oferecendo durabilidade, roteamento flexível e suporte a diversos padrões de mensagens. É amplamente utilizado em arquiteturas distribuídas.
*   **Redis (com cliente .NET StackExchange.Redis):** Uma alternativa leve e de alta performance, ideal para cenários de Pub/Sub onde a persistência de mensagens não é a principal preocupação, mas a baixa latência é. Pode ser usado para um modelo de Pub/Sub simples, como sugerido no projeto de referência.

### 2.4. Módulo de Configuração (Configuration Module)

Um módulo centralizado para gerenciar as configurações da aplicação, como interface de rede, filtros de porta, credenciais da fila de mensagens, etc.

**Responsabilidades:**
*   Carregar configurações de arquivos (e.g., `appsettings.json`).
*   Fornecer acesso tipado às configurações para os outros módulos.

**Tecnologias Sugeridas:**
*   **Sistema de Configuração do .NET:** Utilizar o `Microsoft.Extensions.Configuration` para carregar configurações de `appsettings.json` e variáveis de ambiente, proporcionando uma forma padronizada e flexível de gerenciar as configurações.

## 3. Fluxo de Dados e Interações

O fluxo de dados no sniffer seguirá as seguintes etapas:

1.  **Inicialização:** O Módulo de Configuração carrega as configurações. O Módulo de Captura é inicializado com a interface de rede e filtros definidos.
2.  **Captura:** O Módulo de Captura intercepta pacotes UDP na porta 5056.
3.  **Extração de Payload:** O Módulo de Captura extrai o payload UDP e o encaminha para o Módulo de Parsing.
4.  **Parsing Photon:** O Módulo de Parsing decodifica o payload Photon, transformando os dados binários em objetos C# representativos dos eventos/dados do jogo.
5.  **Publicação:** Os objetos parseados são enviados para o Módulo de Publicação, que os serializa (e.g., para JSON) e os publica na fila de mensagens configurada (RabbitMQ ou Redis).
6.  **Consumo (Externo):** Serviços externos (não parte deste esqueleto inicial) podem consumir as mensagens da fila para processamento posterior.

```mermaid
graph TD
    A[Interface de Rede] --> B{Módulo de Captura de Pacotes}
    B --> C{Módulo de Parsing de Pacotes Photon}
    C --> D{Módulo de Publicação para Fila}
    D --> E[Fila de Mensagens (RabbitMQ/Redis)]
    E --> F[Consumidores (Serviços Externos)]
    G[Configurações (appsettings.json)] --> B
    G --> D
```

## 4. Estrutura do Projeto (C# .NET 8)

O projeto será organizado da seguinte forma:

```
AlbionOnlineSniffer/
├── AlbionOnlineSniffer.sln
├── src/
│   ├── AlbionOnlineSniffer.Core/       # Lógica de negócio principal, modelos de dados Photon
│   │   ├── Models/                     # Classes que representam eventos/dados Photon
│   │   ├── Services/                   # Interfaces e implementações de serviços (e.g., IPhotonParser)
│   │   └── Extensions/                 # Métodos de extensão úteis
│   ├── AlbionOnlineSniffer.Capture/    # Implementação do Módulo de Captura (Pcap.Net)
│   │   ├── PacketCaptureService.cs
│   │   └── Interfaces/PacketCaptureService.cs
│   ├── AlbionOnlineSniffer.Queue/      # Implementação do Módulo de Publicação (RabbitMQ/Redis)
│   │   ├── Publishers/                 # Implementações específicas para RabbitMQ, Redis
│   │   └── Interfaces/IQueuePublisher.cs
│   ├── AlbionOnlineSniffer.App/        # Aplicação console ou serviço Windows/Linux
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   └── AlbionOnlineSniffer.Tests/      # Projeto de testes unitários e de integração
├── README.md
├── .gitignore
└── Dockerfile (opcional, para conteinerização)
```

## 5. Próximos Passos

Com base neste planejamento, os próximos passos serão:

1.  **Configuração do Ambiente:** Garantir que o .NET 8 SDK esteja instalado e que as ferramentas necessárias (e.g., Visual Studio, VS Code) estejam prontas.
2.  **Criação do Projeto Base:** Estruturar o projeto C# conforme a seção 4.
3.  **Implementação do Módulo de Captura:** Integrar o Pcap.Net e desenvolver a lógica de captura e filtragem.
4.  **Desenvolvimento do Parser Photon:** Traduzir a lógica do `PhotonPacketParser.js` para C#.
5.  **Implementação do Publisher:** Configurar e implementar a publicação para RabbitMQ ou Redis.
6.  **Testes:** Desenvolver testes para cada módulo para garantir a funcionalidade e a robustez.
7.  **Documentação:** Detalhar o uso e a configuração do sniffer no `README.md`.

Este planejamento servirá como um guia para a implementação do sniffer, garantindo uma abordagem estruturada e eficiente.

