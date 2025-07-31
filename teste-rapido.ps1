# Teste Rápido UDP - Porta 5050
Write-Host "=== Teste Rápido UDP ===" -ForegroundColor Green
Write-Host "Enviando pacote de teste para porta 5050..." -ForegroundColor Yellow

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Loopback, 5050)
    $message = "Teste rapido - $(Get-Date)"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($message)
    
    $bytesSent = $udpClient.Send($bytes, $bytes.Length, $endPoint)
    Write-Host "Pacote enviado com sucesso!" -ForegroundColor Green
    Write-Host "Bytes enviados: $bytesSent" -ForegroundColor Cyan
    Write-Host "Mensagem: $message" -ForegroundColor Cyan
    
    $udpClient.Close()
}
catch {
    Write-Host "Erro ao enviar pacote: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Certifique-se de que o AlbionOnlineSniffer esta rodando!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Verifique os logs do AlbionOnlineSniffer para confirmar recebimento." -ForegroundColor Cyan 