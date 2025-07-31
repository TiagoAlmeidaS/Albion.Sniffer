# Teste de Pacote Simples - Verificar Validacao
Write-Host "=== Teste de Pacote Simples ===" -ForegroundColor Green
Write-Host "Testando se o pacote passa na validacao do PhotonPacketParser..." -ForegroundColor Yellow

# Enviar pacote que deve passar na validacao
Write-Host ""
Write-Host "Enviando pacote que deve passar na validacao..." -ForegroundColor Cyan

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    
    # Criar pacote que deve passar na validacao do PhotonPacketParser
    # Baseado no metodo IsValidPhotonPacket:
    # 1. Signature (2 bytes) - qualquer valor
    # 2. Message Type (1 byte) - deve ser 0x01 ou 0x02
    # 3. Packet ID (2 bytes) - 29 para NewCharacter
    # 4. Timestamp (4 bytes)
    # 5. Parameter Count (1 byte)
    # 6. Parameters...
    
    $packetBytes = @(
        0x01, 0x02,  # Signature (qualquer valor)
        0x01,         # Message Type (0x01 = evento)
        0x1D, 0x00,  # Packet ID 29 (NewCharacter) - little endian
        0x00, 0x00, 0x00, 0x00,  # Timestamp (4 bytes)
        0x01,         # Parameter Count (1 parameter)
        0x00,         # Parameter Key (1 byte)
        0x0A,         # Parameter Type (0x0A = String)
        0x09,         # String Length (1 byte)
        0x54, 0x65, 0x73, 0x74, 0x56, 0x61, 0x6C, 0x75, 0x65  # "TestValue"
    )
    
    $bytesSent = $udpClient.Send($packetBytes, $packetBytes.Length, $endPoint)
    Write-Host "Pacote enviado: $bytesSent bytes" -ForegroundColor Green
    
    $udpClient.Close()
}
catch {
    Write-Host "Erro ao enviar pacote: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Verificacao ===" -ForegroundColor Yellow
Write-Host "Verifique os logs do AlbionOnlineSniffer para:" -ForegroundColor White
Write-Host "1. '📥 RECEBENDO PACOTE UDP: X bytes'" -ForegroundColor Cyan
Write-Host "2. '✅ PACOTE ENRIQUECIDO: NewCharacter (ID: 29)'" -ForegroundColor Cyan
Write-Host "3. '📦 PROCESSANDO PACOTE: ID 29'" -ForegroundColor Cyan

Write-Host ""
Write-Host "Se nao aparecer o log 2, o problema esta na validacao do PhotonPacketParser!" -ForegroundColor Red
Write-Host "O pacote pode estar sendo rejeitado por:" -ForegroundColor Gray
Write-Host "- Signature incorreta" -ForegroundColor Gray
Write-Host "- Message Type incorreto" -ForegroundColor Gray
Write-Host "- Estrutura do pacote invalida" -ForegroundColor Gray 