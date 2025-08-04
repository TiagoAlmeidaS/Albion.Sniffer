# 🎯 Albion Online Sniffer - Executável

## 📋 Descrição

O **Albion Online Sniffer** é uma aplicação para captura e processamento de eventos do jogo Albion Online. Este executável permite monitorar em tempo real os eventos do jogo e enviá-los para sistemas de mensageria como RabbitMQ.

## 🚀 Como Usar

### Pré-requisitos

- **Windows 10/11** (x64)
- **Privilégios de Administrador** (necessário para captura de pacotes)
- **Albion Online** em execução
- **RabbitMQ** ou **Redis** configurado (opcional)

### Instalação

1. **Baixe o executável** da seção de releases
2. **Extraia o arquivo** para uma pasta de sua preferência
3. **Execute como Administrador** o arquivo `AlbionOnlineSniffer.exe`

### Configuração

O aplicativo usa o arquivo `appsettings.json` para configuração. Principais opções:

```json
{
  "Messaging": {
    "Type": "RabbitMQ", // ou "Redis"
    "ConnectionString": "amqp://localhost:5672",
    "QueueName": "albion-events"
  },
  "Capture": {
    "UdpPort": 5050,
    "EnableBinDumps": true
  }
}
```

## 🔧 Compilação

### Usando o Script de Build

```powershell
# Compilar e executar
.\build-executable.ps1

# Compilar e publicar executável
.\build-executable.ps1 -Publish

# Limpar builds anteriores e compilar
.\build-executable.ps1 -Clean -Publish
```

### Usando dotnet CLI

```bash
# Restaurar dependências
dotnet restore src/AlbionOnlineSniffer.App/AlbionOnlineSniffer.App.csproj

# Compilar
dotnet build src/AlbionOnlineSniffer.App/AlbionOnlineSniffer.App.csproj -c Release

# Publicar executável
dotnet publish src/AlbionOnlineSniffer.App/AlbionOnlineSniffer.App.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true \
  -p:PublishTrimmed=true
```

## 📊 Eventos Capturados

O sniffer captura e processa os seguintes tipos de eventos:

### 🎮 Eventos de Jogadores
- **NewCharacterEvent**: Novo jogador detectado
- **MoveEvent**: Movimento de jogadores
- **HealthUpdateEvent**: Atualização de vida
- **CharacterEquipmentChangedEvent**: Mudança de equipamento
- **MountedEvent**: Montaria/desmontaria

### 🗺️ Eventos de Localização
- **ChangeClusterEvent**: Mudança de cluster
- **LoadClusterObjectsEvent**: Objetivos do cluster
- **MistsPlayerJoinedInfoEvent**: Informações dos Mists

### 🐉 Eventos de Mobs
- **NewMobEvent**: Novo mob detectado
- **MobChangeStateEvent**: Mudança de estado do mob

### 🌿 Eventos de Recursos
- **NewHarvestableEvent**: Novo recurso
- **HarvestableChangeStateEvent**: Mudança de estado do recurso
- **NewHarvestablesListEvent**: Lista de recursos

### 🏰 Eventos de Dungeons
- **NewDungeonEvent**: Nova dungeon
- **NewLootChestEvent**: Novo baú de loot

### 🎣 Eventos de Pesca
- **NewFishingZoneEvent**: Nova zona de pesca

### 🌟 Eventos de Wisps
- **NewGatedWispEvent**: Novo wisp de portal
- **WispGateOpenedEvent**: Portal aberto

## 📡 Formato das Mensagens

Todas as mensagens são enviadas no formato JSON:

```json
{
  "EventType": "NewCharacterEvent",
  "Timestamp": "2024-01-01T00:00:00Z",
  "Data": {
    "Id": 12345,
    "Name": "PlayerName",
    "Guild": "GuildName",
    "Alliance": "AllianceName",
    "Position": {"X": 100.0, "Y": 200.0},
    "Health": 100,
    "Faction": "Martlock"
  }
}
```

## 🔍 Troubleshooting

### Problemas Comuns

1. **Erro de Privilégios**
   ```
   ❌ Erro: Acesso negado para captura de pacotes
   ✅ Solução: Execute como Administrador
   ```

2. **Porta UDP em Uso**
   ```
   ❌ Erro: Porta 5050 já está em uso
   ✅ Solução: Altere a porta no appsettings.json
   ```

3. **Conexão RabbitMQ**
   ```
   ❌ Erro: Não foi possível conectar ao RabbitMQ
   ✅ Solução: Verifique se o RabbitMQ está rodando
   ```

### Logs

O aplicativo gera logs detalhados no console. Principais níveis:
- **INFO**: Informações gerais
- **WARN**: Avisos
- **ERROR**: Erros
- **DEBUG**: Informações detalhadas

## 🛡️ Segurança

- O aplicativo **NÃO** modifica o jogo
- **NÃO** envia dados para servidores externos (exceto RabbitMQ/Redis configurado)
- **NÃO** armazena dados pessoais
- Funciona apenas como **capturador de pacotes de rede**

## 📄 Licença

Este projeto é de código aberto. Veja o arquivo LICENSE para mais detalhes.

## 🤝 Contribuição

Contribuições são bem-vindas! Por favor:
1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📞 Suporte

Para suporte ou dúvidas:
- Abra uma issue no GitHub
- Consulte a documentação
- Verifique os logs de erro

---

**🎯 Albion Online Sniffer** - Capturando eventos em tempo real! 