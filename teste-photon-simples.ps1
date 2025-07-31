# Teste Simples - Simulando Protocolo Photon
Write-Host "=== Teste Simples Photon ===" -ForegroundColor Green
Write-Host "Simulando pacotes do protocolo Photon..." -ForegroundColor Yellow

# Funcao para criar pacote Photon simulado
function Create-PhotonPacket {
    param(
        [int]$PacketId
    )
    
    # Estrutura basica do protocolo Photon
    # 1. Signature (2 bytes) - 0x01 0x02
    # 2. Message Type (1 byte) - 0x01 para evento
    # 3. Packet ID (2 bytes)
    # 4. Timestamp (4 bytes)
    # 5. Parameter Count (1 byte)
    # 6. Parameters...
    
    $packet = @()
    
    # Signature
    $packet += 0x01, 0x02
    
    # Message Type (Event)
    $packet += 0x01
    
    # Packet ID (little endian)
    $packet += [byte]($PacketId -band 0xFF)
    $packet += [byte](($PacketId -shr 8) -band 0xFF)
    
    # Timestamp (4 bytes)
    $timestamp = [int]([DateTimeOffset]::Now.ToUnixTimeSeconds())
    $packet += [byte]($timestamp -band 0xFF)
    $packet += [byte](($timestamp -shr 8) -band 0xFF)
    $packet += [byte](($timestamp -shr 16) -band 0xFF)
    $packet += [byte](($timestamp -shr 24) -band 0xFF)
    
    # Parameter Count (1 byte)
    $packet += 0x01  # 1 parameter
    
    # Parameter Key (1 byte)
    $packet += 0x00
    
    # Parameter Type (1 byte) - String
    $packet += 0x0A  # String type
    
    # Parameter Value (String)
    $value = "TestValue"
    $valueBytes = [System.Text.Encoding]::UTF8.GetBytes($value)
    $packet += [byte]($valueBytes.Length)
    $packet += $valueBytes
    
    return $packet
}

# Teste 1: Pacote NewCharacter (ID 29)
Write-Host ""
Write-Host "1. Testando pacote NewCharacter (ID 29)..." -ForegroundColor Cyan

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    
    $packetBytes = Create-PhotonPacket -PacketId 29
    
    $bytesSent = $udpClient.Send($packetBytes, $packetBytes.Length, $endPoint)
    Write-Host "Pacote NewCharacter enviado: $bytesSent bytes" -ForegroundColor Green
    
    $udpClient.Close()
}
catch {
    Write-Host "Erro ao enviar pacote NewCharacter: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

# Teste 2: Pacote Move (ID 3)
Write-Host ""
Write-Host "2. Testando pacote Move (ID 3)..." -ForegroundColor Cyan

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    
    $packetBytes = Create-PhotonPacket -PacketId 3
    
    $bytesSent = $udpClient.Send($packetBytes, $packetBytes.Length, $endPoint)
    Write-Host "Pacote Move enviado: $bytesSent bytes" -ForegroundColor Green
    
    $udpClient.Close()
}
catch {
    Write-Host "Erro ao enviar pacote Move: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Instrucoes de Verificacao ===" -ForegroundColor Yellow
Write-Host "1. Verifique os logs do AlbionOnlineSniffer para mensagens como:" -ForegroundColor White
Write-Host "   'DISPARANDO EVENTO: NewCharacter'" -ForegroundColor Cyan
Write-Host "   'DISPARANDO EVENTO: Move'" -ForegroundColor Cyan
Write-Host "   'EVENTO RECEBIDO: NewCharacter'" -ForegroundColor Cyan
Write-Host "   'PUBLICANDO: NewCharacter -> albion.event.newcharacter'" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Se nao aparecer essas mensagens, verifique:" -ForegroundColor White
Write-Host "   - Se o AlbionOnlineSniffer esta rodando" -ForegroundColor Gray
Write-Host "   - Se a conexao com RabbitMQ esta funcionando" -ForegroundColor Gray
Write-Host "   - Se ha erros nos logs" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Para verificar no RabbitMQ:" -ForegroundColor White
Write-Host "   - Acesse: https://cow.rmq2.cloudamqp.com/" -ForegroundColor Cyan
Write-Host "   - Login: eioundda" -ForegroundColor Cyan
Write-Host "   - Verifique as filas e exchanges" -ForegroundColor Cyan 