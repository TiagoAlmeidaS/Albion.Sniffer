# Teste de validação de pacotes Photon
# Simula pacotes reais do Albion Online

Write-Host "🧪 TESTE DE VALIDACAO PHOTON" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Pacote de teste 1: Evento válido (0x01, 0x00)
$eventPacket = @(
    0x01, 0x00,  # Message Type: Event, SubType: 0
    0x00, 0x01,  # Packet ID: 1
    0x00, 0x00, 0x00, 0x00,  # Timestamp: 0
    0x02,        # Parameter Count: 2
    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08  # Dados extras
)

# Pacote de teste 2: Operação válida (0x02, 0x01)
$operationPacket = @(
    0x02, 0x01,  # Message Type: Operation, SubType: 1
    0x00, 0x02,  # Packet ID: 2
    0x00, 0x00, 0x00, 0x00,  # Timestamp: 0
    0x01,        # Parameter Count: 1
    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08  # Dados extras
)

# Pacote de teste 3: Inválido (tipo incorreto)
$invalidPacket = @(
    0x03, 0x00,  # Message Type: Inválido
    0x00, 0x03,  # Packet ID: 3
    0x00, 0x00, 0x00, 0x00,  # Timestamp: 0
    0x01,        # Parameter Count: 1
    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08  # Dados extras
)

# Pacote de teste 4: Inválido (subtipo incorreto)
$invalidSubTypePacket = @(
    0x01, 0x01,  # Message Type: Event, SubType: 1 (deveria ser 0)
    0x00, 0x04,  # Packet ID: 4
    0x00, 0x00, 0x00, 0x00,  # Timestamp: 0
    0x01,        # Parameter Count: 1
    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08  # Dados extras
)

# Pacote de teste 5: Muito pequeno
$smallPacket = @(0x01, 0x00, 0x00, 0x01)

function Test-Packet {
    param([byte[]]$packet, [string]$description)
    
    Write-Host "`n🔍 Testando: $description" -ForegroundColor Yellow
    Write-Host "Dados: $([System.BitConverter]::ToString($packet))" -ForegroundColor Gray
    
    # Simular o teste enviando para o sniffer
    # Aqui você pode adicionar código para enviar o pacote para o sniffer
    Write-Host "Pacote enviado para validação..." -ForegroundColor Green
}

# Executar testes
Test-Packet -packet $eventPacket -description "Evento válido (0x01, 0x00)"
Test-Packet -packet $operationPacket -description "Operação válida (0x02, 0x01)"
Test-Packet -packet $invalidPacket -description "Tipo inválido (0x03, 0x00)"
Test-Packet -packet $invalidSubTypePacket -description "Subtipo inválido (0x01, 0x01)"
Test-Packet -packet $smallPacket -description "Pacote muito pequeno"

Write-Host "`n✅ Testes concluídos!" -ForegroundColor Green
Write-Host "Verifique os logs do sniffer para ver os resultados da validação." -ForegroundColor Cyan 