# Teste de Pacotes UDP - Porta 5050

Este documento explica como testar o sistema AlbionOnlineSniffer enviando pacotes UDP simulados para a porta 5050.

## 🎯 Objetivo

Verificar se o sistema está funcionando corretamente enviando pacotes de teste para a porta 5050, onde o AlbionOnlineSniffer está escutando.

## 🛠️ Ferramentas Disponíveis

### 1. Script PowerShell (Mais Simples)

O arquivo `test-packet-sender.ps1` é a forma mais rápida de testar:

```powershell
# Executar o script
.\test-packet-sender.ps1

# Ou com parâmetros
.\test-packet-sender.ps1 -Count 10 -Delay 500 -Message "Teste customizado"
```

**Opções disponíveis:**
- **1**: Enviar mensagem simples
- **2**: Enviar eventos simulados do Albion Online
- **3**: Enviar múltiplos pacotes de teste
- **4**: Modo interativo (enviar pacotes continuamente)

### 2. Programa C# (Mais Avançado)

Para usar o programa C#, você tem duas opções:

#### Opção A: Projeto Separado (Recomendado)
```bash
# Compilar e executar o projeto de teste separado
dotnet run --project TestPacketSender.csproj

# Ou usar o script de build
.\build-test-sender.ps1
```

#### Opção B: Modo de Teste Integrado
```bash
# Executar o programa principal em modo de teste
dotnet run --project src/AlbionOnlineSniffer.App/AlbionOnlineSniffer.App.csproj -- --test-mode
```

### 3. Teste Manual com Netcat (Linux/macOS)

```bash
# Enviar uma mensagem simples
echo "Teste manual" | nc -u localhost 5050

# Enviar JSON simulado
echo '{"EventType":"TestEvent","Data":{"message":"teste"}}' | nc -u localhost 5050
```

## 📋 Passos para Testar

### Passo 1: Iniciar o AlbionOnlineSniffer

Primeiro, certifique-se de que o AlbionOnlineSniffer está rodando:

```bash
# Em um terminal
dotnet run --project src/AlbionOnlineSniffer.App/AlbionOnlineSniffer.App.csproj
```

Você deve ver logs como:
```
[INFO] Iniciando AlbionOnlineSniffer...
[INFO] Iniciando captura de pacotes na porta UDP 5050...
[INFO] Sniffer iniciado. Sistema de eventos funcionando.
```

### Passo 2: Executar o Teste

Em outro terminal, execute o script de teste:

```powershell
.\test-packet-sender.ps1
```

### Passo 3: Verificar os Resultados

No terminal do AlbionOnlineSniffer, você deve ver logs indicando que os pacotes foram recebidos:

```
[DEBUG] Pacote UDP recebido: X bytes
[INFO] Evento processado: TestEvent
[DEBUG] Evento publicado na fila: TestEvent -> albion.event.testevent
```

## 🔍 Tipos de Teste Disponíveis

### 1. Teste Simples
Envia uma mensagem de texto simples para verificar se a captura básica está funcionando.

### 2. Eventos Simulados do Albion Online
Envia eventos que simulam dados reais do jogo:
- **NewCharacter**: Novo jogador entrando no jogo
- **Move**: Movimento de personagem
- **Combat**: Evento de combate

### 3. Múltiplos Pacotes
Envia uma sequência de pacotes com delay configurável para testar o processamento contínuo.

### 4. Modo Interativo
Permite enviar mensagens customizadas em tempo real.

## 🐛 Troubleshooting

### Problema: "Nenhum pacote recebido"

**Possíveis causas:**
1. AlbionOnlineSniffer não está rodando
2. Firewall bloqueando a porta 5050
3. Programa não está executando como Administrador (Windows)

**Soluções:**
```powershell
# Verificar se a porta está em uso
netstat -an | findstr :5050

# Verificar se o processo está rodando
tasklist | findstr AlbionOnlineSniffer
```

### Problema: "Erro de permissão"

**Solução (Windows):**
```powershell
# Executar PowerShell como Administrador
Start-Process powershell -Verb RunAs
```

### Problema: "Npcap não encontrado"

**Solução:**
1. Baixar e instalar Npcap: https://npcap.com/
2. Reiniciar o computador
3. Executar o programa como Administrador

## 📊 Exemplos de Uso

### Teste Rápido
```powershell
# Enviar 5 pacotes com 1 segundo de intervalo
.\test-packet-sender.ps1 -Count 5 -Delay 1000
```

### Teste de Estresse
```powershell
# Enviar muitos pacotes rapidamente
.\test-packet-sender.ps1 -Count 100 -Delay 100
```

### Teste de Eventos Específicos
```powershell
# Escolher opção 2 para eventos simulados do Albion Online
.\test-packet-sender.ps1
# Depois escolher "2"
```

## 🔧 Configuração Avançada

### Alterar Porta de Destino

Para testar em uma porta diferente, edite o script:

```powershell
# No script, altere a linha:
[int]$Port = 5050
# Para:
[int]$Port = 5051
```

### Teste com Diferentes Formatos

O sistema aceita diferentes formatos de dados:
- Texto simples
- JSON
- Dados binários

### Monitoramento em Tempo Real

Para monitorar os logs em tempo real:

```bash
# Linux/macOS
tail -f logs/albion-sniffer.log

# Windows PowerShell
Get-Content logs/albion-sniffer.log -Wait
```

## ✅ Checklist de Verificação

- [ ] AlbionOnlineSniffer está rodando
- [ ] Porta 5050 está livre
- [ ] Firewall não está bloqueando
- [ ] Programa executando como Administrador (Windows)
- [ ] Npcap instalado (Windows)
- [ ] Logs mostram captura de pacotes
- [ ] Eventos estão sendo processados
- [ ] Mensagens estão sendo publicadas na fila

## 📝 Logs Esperados

### Logs de Sucesso
```
[INFO] Pacote UDP recebido: 45 bytes
[DEBUG] Processando evento: TestEvent
[INFO] Evento publicado na fila: TestEvent -> albion.event.testevent
[DEBUG] Métricas de captura atualizadas
```

### Logs de Erro Comuns
```
[ERROR] Erro ao capturar pacote: Access denied
[WARNING] Nenhum adaptador de rede encontrado
[ERROR] Falha ao conectar com RabbitMQ
```

## 🎯 Próximos Passos

Após confirmar que os testes estão funcionando:

1. Teste com dados reais do Albion Online
2. Configure o RabbitMQ para receber os eventos
3. Implemente handlers específicos para seus casos de uso
4. Configure monitoramento e alertas 