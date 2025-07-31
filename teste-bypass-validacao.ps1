# Teste Bypass Validacao - Pacote Mais Simples
Write-Host "=== Teste Bypass Validacao ===" -ForegroundColor Green
Write-Host "Testando com pacote mais simples..." -ForegroundColor Yellow

# Enviar pacote mais simples
Write-Host ""
Write-Host "Enviando pacote mais simples..." -ForegroundColor Cyan

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    
    # Pacote mais simples - apenas os bytes essenciais
    $packetBytes = @(
        0x01, 0x02,  # Signature (qualquer valor)
        0x01,         # Message Type (0x01 = evento)
        0x1D, 0x00,  # Packet ID 29 (NewCharacter) - little endian
        0x00, 0x00, 0x00, 0x00,  # Timestamp (4 bytes)
        0x00          # Parameter Count (0 parameters - mais simples)
    )
    
    $bytesSent = $udpClient.Send($packetBytes, $packetBytes.Length, $endPoint)
    Write-Host "Pacote simples enviado: $bytesSent bytes" -ForegroundColor Green
    
    $udpClient.Close()
}
catch {
    Write-Host "Erro ao enviar pacote: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Verificacao ===" -ForegroundColor Yellow
Write-Host "Verifique os logs do AlbionOnlineSniffer para:" -ForegroundColor White
Write-Host "1. '📥 RECEBENDO PACOTE UDP: X bytes'" -ForegroundColor Cyan
Write-Host "2. '🔍 PARSEANDO PACOTE: X bytes'" -ForegroundColor Cyan
Write-Host "3. '🔍 VERIFICANDO VALIDACAO PHOTON...'" -ForegroundColor Cyan
Write-Host "4. '🔍 Signature: [01-02]'" -ForegroundColor Cyan
Write-Host "5. '🔍 Message Type: 0x01'" -ForegroundColor Cyan
Write-Host "6. '🔍 Validacao: True'" -ForegroundColor Cyan
Write-Host "7. '✅ Validacao Photon passou!'" -ForegroundColor Cyan

Write-Host ""
Write-Host "Se parar no log 3 ou 4, o problema esta na validacao!" -ForegroundColor Red
Write-Host "Se passar da validacao mas parar depois, o problema esta na estrutura!" -ForegroundColor Red 