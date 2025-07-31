# Teste de Pacotes Binarios - Simulando Protocolo Photon
Write-Host "=== Teste de Pacotes Binarios ===" -ForegroundColor Green
Write-Host "Simulando pacotes do protocolo Photon..." -ForegroundColor Yellow

# Funcao para criar pacote binario simulado
function Create-PhotonPacket {
    param(
        [int]$PacketId,
        [hashtable]$Parameters
    )
    
    # Estrutura basica de um pacote Photon
    # Header: 4 bytes para ID do pacote
    $header = [System.BitConverter]::GetBytes($PacketId)
    
    # Para simplicidade, vamos criar um payload JSON como string
    $jsonPayload = $Parameters | ConvertTo-Json -Compress
    $payloadBytes = [System.Text.Encoding]::UTF8.GetBytes($jsonPayload)
    
    # Combinar header + payload
    $packet = @()
    $packet += $header
    $packet += $payloadBytes
    
    return $packet
}

# Teste 1: Pacote NewCharacter (ID 29)
Write-Host ""
Write-Host "1. Testando pacote NewCharacter (ID 29)..." -ForegroundColor Cyan

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    
    # Criar pacote simulado
    $parameters = @{
        "0" = 12345  # Player ID
        "1" = "TestPlayer"  # Name
        "2" = @{ X = 100; Y = 200 }  # Position
    }
    
    $packetBytes = Create-PhotonPacket -PacketId 29 -Parameters $parameters
    
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
    
    # Criar pacote simulado
    $parameters = @{
        "0" = 12345  # Player ID
        "1" = @{ X = 150; Y = 250 }  # New Position
        "2" = 5.5  # Speed
    }
    
    $packetBytes = Create-PhotonPacket -PacketId 3 -Parameters $parameters
    
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