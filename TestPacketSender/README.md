# TestPacketSender

Programa de teste para enviar pacotes UDP simulados para a porta 5050 do AlbionOnlineSniffer.

## 🎯 Objetivo

Este programa permite testar o sistema AlbionOnlineSniffer enviando pacotes UDP simulados para verificar se a captura e processamento estão funcionando corretamente.

## 🚀 Como Usar

### Pré-requisitos

1. **AlbionOnlineSniffer rodando**: Certifique-se de que o sniffer principal está executando
2. **.NET 8.0**: O projeto requer .NET 8.0 ou superior

### Execução

#### Opção 1: Execução Direta
```bash
dotnet run --project TestPacketSender.csproj
```

#### Opção 2: Usando o Script
```powershell
.\build-test-sender.ps1
```

#### Opção 3: Compilar e Executar Manualmente
```bash
dotnet build TestPacketSender.csproj
dotnet run --project TestPacketSender.csproj
```

## 📋 Opções de Teste

Quando executado, o programa oferece 4 opções:

### 1. Enviar pacotes de teste simples
- Envia múltiplos pacotes com dados de teste
- Permite configurar quantidade e delay entre pacotes
- Útil para verificar se a captura básica está funcionando

### 2. Enviar eventos simulados do Albion Online
- Simula eventos reais do jogo:
  - **NewCharacter**: Novo jogador entrando
  - **Move**: Movimento de personagem
  - **Combat**: Evento de combate
- Útil para testar processamento de eventos específicos

### 3. Enviar pacote de texto customizado
- Permite enviar uma mensagem personalizada
- Útil para testes específicos

### 4. Modo interativo
- Permite enviar mensagens continuamente
- Digite 'quit' para sair
- Útil para testes em tempo real

## 🔍 Verificação

Após enviar pacotes, verifique os logs do AlbionOnlineSniffer para confirmar recebimento:

```
[DEBUG] Pacote UDP recebido: X bytes
[INFO] Evento processado: TestEvent
[DEBUG] Evento publicado na fila: TestEvent -> albion.event.testevent
```

## 🐛 Troubleshooting

### Problema: "Erro ao enviar pacote"
- Verifique se o AlbionOnlineSniffer está rodando
- Confirme se a porta 5050 está livre
- Verifique se não há firewall bloqueando

### Problema: "Compilação falhou"
- Verifique se o .NET 8.0 está instalado
- Execute `dotnet restore` antes de compilar
- Verifique se todos os arquivos estão presentes

## 📁 Estrutura do Projeto

```
TestPacketSender/
├── TestPacketSender.csproj    # Arquivo de projeto
├── Program.cs                 # Programa principal
└── README.md                 # Esta documentação
```

## 🔧 Configuração

### Alterar Porta de Destino

Para testar em uma porta diferente, edite o arquivo `Program.cs`:

```csharp
// Linha 158
var targetEndPoint = new IPEndPoint(IPAddress.Loopback, 5050);
// Altere para:
var targetEndPoint = new IPEndPoint(IPAddress.Loopback, 5051);
```

### Alterar Host de Destino

Para testar em um host diferente:

```csharp
// Linha 158
var targetEndPoint = new IPEndPoint(IPAddress.Loopback, 5050);
// Altere para:
var targetEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 5050);
```

## 📊 Exemplos de Uso

### Teste Rápido
```bash
# Executar e escolher opção 1
dotnet run --project TestPacketSender.csproj
# Digite: 1
# Quantos pacotes: 5
# Delay: 1000
```

### Teste de Eventos do Jogo
```bash
# Executar e escolher opção 2
dotnet run --project TestPacketSender.csproj
# Digite: 2
```

### Teste Interativo
```bash
# Executar e escolher opção 4
dotnet run --project TestPacketSender.csproj
# Digite: 4
# Digite mensagens e pressione Enter
# Digite 'quit' para sair
```

## 🎯 Próximos Passos

Após confirmar que os testes estão funcionando:

1. Teste com dados reais do Albion Online
2. Configure handlers específicos para seus casos de uso
3. Implemente monitoramento e alertas
4. Configure o RabbitMQ para receber os eventos 