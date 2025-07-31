# Teste de Mensageria - Verificar se eventos estao sendo enviados
Write-Host "=== Teste de Mensageria ===" -ForegroundColor Green
Write-Host "Verificando se eventos estao sendo enviados para as filas..." -ForegroundColor Yellow

# Teste 1: Enviar pacote que deve gerar evento NewCharacter
Write-Host ""
Write-Host "1. Testando evento NewCharacter..." -ForegroundColor Cyan

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    
    # Simular pacote NewCharacter (formato simplificado)
    $testData = @{
        EventType = "NewCharacter"
        PlayerId = 12345
        Name = "TestPlayer"
        Position = @{ X = 100; Y = 200 }
    }
    
    $json = $testData | ConvertTo-Json -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    
    $bytesSent = $udpClient.Send($bytes, $bytes.Length, $endPoint)
    Write-Host "Pacote NewCharacter enviado: $bytesSent bytes" -ForegroundColor Green
    
    $udpClient.Close()
}
catch {
    Write-Host "Erro ao enviar pacote NewCharacter: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 2

# Teste 2: Enviar pacote que deve gerar evento Move
Write-Host ""
Write-Host "2. Testando evento Move..." -ForegroundColor Cyan

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    
    # Simular pacote Move
    $testData = @{
        EventType = "Move"
        PlayerId = 12345
        Position = @{ X = 150; Y = 250 }
        Speed = 5.5
    }
    
    $json = $testData | ConvertTo-Json -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    
    $bytesSent = $udpClient.Send($bytes, $bytes.Length, $endPoint)
    Write-Host "Pacote Move enviado: $bytesSent bytes" -ForegroundColor Green
    
    $udpClient.Close()
}
catch {
    Write-Host "Erro ao enviar pacote Move: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Instrucoes de Verificacao ===" -ForegroundColor Yellow
Write-Host "1. Verifique os logs do AlbionOnlineSniffer para mensagens como:" -ForegroundColor White
Write-Host "   'Evento publicado na fila: NewCharacter -> albion.event.newcharacter'" -ForegroundColor Cyan
Write-Host "   'Evento publicado na fila: Move -> albion.event.move'" -ForegroundColor Cyan
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