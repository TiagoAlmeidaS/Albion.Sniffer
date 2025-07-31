# Exemplo rápido para testar o AlbionOnlineSniffer
# Este script envia um pacote simples para verificar se o sistema está funcionando

Write-Host "=== Teste Rápido - AlbionOnlineSniffer ===" -ForegroundColor Green
Write-Host "Enviando pacote de teste para porta 5050..." -ForegroundColor Yellow

try {
    $udpClient = New-Object System.Net.Sockets.UdpClient
    $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Parse("localhost"), 5050)
    $message = "Teste rápido - $(Get-Date)"
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($message)
    
    $bytesSent = $udpClient.Send($bytes, $bytes.Length, $endPoint)
    Write-Host "✓ Pacote enviado com sucesso!" -ForegroundColor Green
    Write-Host "  Bytes enviados: $bytesSent" -ForegroundColor Cyan
    Write-Host "  Mensagem: $message" -ForegroundColor Cyan
    
    $udpClient.Close()
}
catch {
    Write-Host "✗ Erro ao enviar pacote: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Certifique-se de que o AlbionOnlineSniffer está rodando!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Verifique os logs do AlbionOnlineSniffer para confirmar recebimento." -ForegroundColor Cyan 