# Teste Direto do PublishAsync
Write-Host "=== Teste Direto do PublishAsync ===" -ForegroundColor Green
Write-Host "Testando se o RabbitMqPublisher esta funcionando..." -ForegroundColor Yellow

# Simular um evento diretamente
$testEvent = @{
    EventType = "TestEvent"
    Timestamp = [DateTimeOffset]::Now.ToUnixTimeSeconds()
    Data = @{
        TestProperty = "TestValue"
        TestNumber = 123
    }
}

$topic = "albion.event.testevent"
$message = @{
    EventType = $testEvent.EventType
    Timestamp = $testEvent.Timestamp
    Data = $testEvent
}

Write-Host "Topic: $topic" -ForegroundColor Cyan
Write-Host "Message: $($message | ConvertTo-Json -Depth 3)" -ForegroundColor Cyan

# Testar conexao com RabbitMQ
Write-Host ""
Write-Host "Para testar manualmente:" -ForegroundColor Yellow
Write-Host "1. Acesse: https://cow.rmq2.cloudamqp.com/" -ForegroundColor Cyan
Write-Host "2. Login: eioundda" -ForegroundColor Cyan
Write-Host "3. Verifique se ha mensagens na exchange 'albion.sniffer'" -ForegroundColor Cyan
Write-Host "4. Verifique se ha mensagens no topic 'albion.event.testevent'" -ForegroundColor Cyan

Write-Host ""
Write-Host "Se nao houver mensagens, o problema pode ser:" -ForegroundColor Red
Write-Host "- Conexao com RabbitMQ falhando" -ForegroundColor Gray
Write-Host "- Credenciais incorretas" -ForegroundColor Gray
Write-Host "- Exchange nao criada" -ForegroundColor Gray
Write-Host "- Permissoes insuficientes" -ForegroundColor Gray 