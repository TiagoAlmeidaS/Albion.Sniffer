# Troubleshooting - AlbionOnlineSniffer

## Erro: "Unable to open the adapter '\Device\NPF_Loopback'"

Este erro indica que o programa está tentando usar o adaptador de loopback, que não é adequado para captura de pacotes de rede.

### Causas Possíveis:

1. **Npcap/WinPcap não instalado**
2. **Programa não executado como Administrador**
3. **Firewall bloqueando acesso**
4. **Adaptadores de rede não disponíveis**

### Soluções:

#### 1. Instalar Npcap
- Baixe e instale o Npcap: https://npcap.com/
- Durante a instalação, certifique-se de marcar "Install Npcap in WinPcap API-compatible Mode"
- Reinicie o computador após a instalação

#### 2. Executar como Administrador
- Clique com o botão direito no executável
- Selecione "Executar como administrador"
- Ou execute o PowerShell/CMD como administrador e rode o programa de lá

#### 3. Verificar Firewall
- Abra o Windows Defender Firewall
- Verifique se o programa tem permissão para acessar a rede
- Temporariamente desabilite o firewall para teste

#### 4. Verificar Adaptadores de Rede
- Abra "Gerenciador de Dispositivos"
- Expanda "Adaptadores de Rede"
- Certifique-se de que há pelo menos um adaptador ativo (Wi-Fi ou Ethernet)

### Debug

O programa agora mostra informações detalhadas sobre:
- Número de dispositivos disponíveis
- Lista de todos os adaptadores encontrados
- Endereços IP de cada adaptador
- Qual adaptador está sendo usado

### Comandos Úteis

Para verificar se o Npcap está instalado:
```cmd
ipconfig /all
```

Para verificar permissões de rede:
```cmd
netsh interface show interface
```

### Teste de Adaptadores

Execute o script de teste para verificar os adaptadores disponíveis:

```cmd
# Compilar o teste
dotnet build test_adapters.csproj

# Executar como administrador
dotnet run --project test_adapters.csproj
```

Este teste mostrará:
- Quantos dispositivos estão disponíveis
- Detalhes de cada dispositivo
- Se as estratégias de seleção funcionam
- Se há problemas com Npcap

### Logs

O programa agora exibe logs detalhados que ajudam a identificar:
- Quais adaptadores estão disponíveis
- Qual adaptador foi selecionado
- Se há problemas de permissão
- Se o filtro de captura foi aplicado corretamente

### Contato

Se o problema persistir, verifique:
1. Os logs exibidos pelo programa
2. Se o Albion Online está rodando
3. Se há outros programas usando captura de pacotes
4. Se o antivírus está interferindo 