# Teste de Evento Forcado - Verificar Integracao
Write-Host "=== Teste de Evento Forcado ===" -ForegroundColor Green
Write-Host "Forcando disparo de evento para testar integracao..." -ForegroundColor Yellow

# Enviar pacote que deve ser reconhecido como NewCharacter
Write-Host ""
Write-Host "Enviando pacote que deve gerar evento NewCharacter..." -ForegroundColor Cyan

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    
    # Criar pacote simples que pode ser reconhecido
    $packetBytes = @(
        0x01, 0x02,  # Signature
        0x01,         # Message Type (Event)
        0x1D, 0x00,  # Packet ID 29 (NewCharacter) - little endian
        0x00, 0x00, 0x00, 0x00,  # Timestamp
        0x01,         # Parameter Count
        0x00,         # Parameter Key
        0x0A,         # Parameter Type (String)
        0x09,         # String Length
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
Write-Host "4. '🎯 TIPO IDENTIFICADO: NewCharacter (ID: 29)'" -ForegroundColor Cyan
Write-Host "5. '🚀 DISPARANDO EVENTO: NewCharacter'" -ForegroundColor Cyan
Write-Host "6. '🎯 EVENTO RECEBIDO: NewCharacter'" -ForegroundColor Cyan
Write-Host "7. '📤 PUBLICANDO: NewCharacter -> albion.event.newcharacter'" -ForegroundColor Cyan
Write-Host "8. '[RabbitMQ] Publicando: albion.event.newcharacter'" -ForegroundColor Cyan
Write-Host "9. '[RabbitMQ] ✅ Mensagem publicada com sucesso'" -ForegroundColor Cyan

Write-Host ""
Write-Host "Se algum desses logs nao aparecer, o problema esta nesse ponto!" -ForegroundColor Red 