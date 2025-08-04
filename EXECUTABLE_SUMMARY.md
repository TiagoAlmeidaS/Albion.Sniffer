# 🎯 Albion Online Sniffer - Executável Configurado

## ✅ Status: CONCLUÍDO

O executável do **Albion Online Sniffer** foi configurado e compilado com sucesso!

## 📁 Localização do Executável

```
dist/AlbionOnlineSniffer-Published/AlbionOnlineSniffer.App.exe
```

## 📊 Informações do Executável

- **Tamanho**: ~38.6 MB
- **Arquitetura**: x64 (Windows)
- **Framework**: .NET 8.0 Self-Contained
- **Tipo**: Single File Executable
- **Otimizações**: Ready-to-Run + Trimmed

## 🎨 Recursos Integrados

### ✅ Logo e Branding
- Logo do projeto integrada (`logo_sniffer.png`)
- Informações de assembly configuradas
- Metadados do aplicativo definidos

### ✅ Configurações de Assembly
- **Título**: Albion Online Sniffer
- **Descrição**: Sniffer para captura de eventos do Albion Online
- **Empresa**: Albion Sniffer Project
- **Versão**: 1.0.0.0
- **Copyright**: Copyright © 2024

### ✅ Otimizações
- **PublishSingleFile**: true
- **SelfContained**: true
- **PublishReadyToRun**: true
- **PublishTrimmed**: true
- **RuntimeIdentifier**: win-x64

## 📦 Arquivos Incluídos

```
dist/AlbionOnlineSniffer-Published/
├── AlbionOnlineSniffer.App.exe (38.6 MB)
├── appsettings.json
├── Assets/
│   └── logo_sniffer.png
├── ao-bin-dumps/
└── src/
    └── AlbionOnlineSniffer.Core/Data/jsons/
```

## 🚀 Como Usar

### Execução Direta
```bash
cd dist/AlbionOnlineSniffer-Published
./AlbionOnlineSniffer.App.exe
```

### Execução como Administrador (Recomendado)
```bash
# Execute como Administrador para captura de pacotes
```

## ⚙️ Configuração

O executável usa o arquivo `appsettings.json` para configuração:

```json
{
  "Messaging": {
    "Type": "RabbitMQ",
    "ConnectionString": "amqp://localhost:5672",
    "QueueName": "albion-events"
  },
  "Capture": {
    "UdpPort": 5050,
    "EnableBinDumps": true
  }
}
```

## 🔧 Recursos Técnicos

### Eventos Capturados
- ✅ NewCharacterEvent
- ✅ MoveEvent
- ✅ HealthUpdateEvent
- ✅ ChangeClusterEvent
- ✅ NewMobEvent
- ✅ NewHarvestableEvent
- ✅ NewDungeonEvent
- ✅ NewLootChestEvent
- ✅ E muito mais...

### Formato das Mensagens
```json
{
  "EventType": "NewCharacterEvent",
  "Timestamp": "2024-01-01T00:00:00Z",
  "Data": {
    "Id": 12345,
    "Name": "PlayerName",
    "Guild": "GuildName",
    "Position": {"X": 100.0, "Y": 200.0}
  }
}
```

## 📋 Pré-requisitos

- **Windows 10/11** (x64)
- **Privilégios de Administrador** (para captura de pacotes)
- **Albion Online** em execução
- **RabbitMQ/Redis** (opcional, para mensageria)

## 🛡️ Segurança

- ✅ **NÃO** modifica o jogo
- ✅ **NÃO** envia dados para servidores externos (exceto configurado)
- ✅ **NÃO** armazena dados pessoais
- ✅ Funciona apenas como capturador de pacotes de rede

## 📞 Suporte

Para suporte ou dúvidas:
- Consulte o `EXECUTABLE_README.md`
- Verifique os logs de erro
- Abra uma issue no GitHub

---

## 🎉 Executável Pronto para Uso!

O **Albion Online Sniffer** está configurado e pronto para capturar eventos do Albion Online em tempo real!

**Localização**: `dist/AlbionOnlineSniffer-Published/AlbionOnlineSniffer.App.exe` 