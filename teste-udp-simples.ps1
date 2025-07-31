# Script simples para testar pacotes UDP na porta 5050
# Este script não depende do build do projeto principal

Write-Host "=== Teste UDP Simples - Porta 5050 ===" -ForegroundColor Green
Write-Host "Certifique-se de que o AlbionOnlineSniffer está rodando!" -ForegroundColor Yellow
Write-Host ""

# Função para enviar pacote UDP
function Send-UdpPacket {
    param(
        [string]$Message,
        [string]$TargetHost = "localhost",
        [int]$Port = 5050
    )
    
    try {
        $udpClient = New-Object System.Net.Sockets.UdpClient
        $endPoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Parse($TargetHost), $Port)
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Message)
        
        $bytesSent = $udpClient.Send($bytes, $bytes.Length, $endPoint)
        Write-Host "✓ Pacote enviado: $bytesSent bytes - '$Message'" -ForegroundColor Green
        
        $udpClient.Close()
        return $true
    }
    catch {
        Write-Host "✗ Erro ao enviar pacote: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Função para enviar evento simulado
function Send-SimulatedEvent {
    param(
        [string]$EventType,
        [object]$EventData
    )
    
    $event = @{
        EventType = $EventType
        Timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
        Data = $EventData
    }
    
    $json = $event | ConvertTo-Json -Compress
    Send-UdpPacket -Message $json
}

Write-Host "Escolha o tipo de teste:" -ForegroundColor Cyan
Write-Host "1. Enviar mensagem simples"
Write-Host "2. Enviar eventos simulados do Albion Online"
Write-Host "3. Enviar múltiplos pacotes de teste"
Write-Host "4. Modo interativo"
Write-Host ""

$choice = Read-Host "Digite sua escolha (1-4)"

switch ($choice) {
    "1" {
        $customMessage = Read-Host "Digite a mensagem (ou pressione Enter para usar padrão)"
        if ([string]::IsNullOrEmpty($customMessage)) {
            $customMessage = "Teste simples - $(Get-Date)"
        }
        Send-UdpPacket -Message $customMessage
    }
    
    "2" {
        Write-Host "Enviando eventos simulados do Albion Online..." -ForegroundColor Yellow
        
        # Evento de novo jogador
        Send-SimulatedEvent -EventType "NewCharacter" -EventData @{
            CharacterId = [System.Guid]::NewGuid().ToString()
            Name = "TestPlayer"
            Position = @{ X = 100; Y = 200; Z = 0 }
            Guild = "TestGuild"
        }
        
        Start-Sleep -Milliseconds 500
        
        # Evento de movimento
        Send-SimulatedEvent -EventType "Move" -EventData @{
            CharacterId = [System.Guid]::NewGuid().ToString()
            Position = @{ X = 150; Y = 250; Z = 0 }
            Speed = 5.5
        }
        
        Start-Sleep -Milliseconds 500
        
        # Evento de combate
        Send-SimulatedEvent -EventType "Combat" -EventData @{
            AttackerId = [System.Guid]::NewGuid().ToString()
            TargetId = [System.Guid]::NewGuid().ToString()
            Damage = 150
            Skill = "Fireball"
        }
        
        Write-Host "Eventos simulados enviados!" -ForegroundColor Green
    }
    
    "3" {
        $count = Read-Host "Quantos pacotes enviar? (padrão: 5)"
        if ([string]::IsNullOrEmpty($count)) { $count = 5 }
        else { $count = [int]$count }
        
        $delay = Read-Host "Delay entre pacotes em ms? (padrão: 1000)"
        if ([string]::IsNullOrEmpty($delay)) { $delay = 1000 }
        else { $delay = [int]$delay }
        
        Write-Host "Enviando $count pacotes com delay de ${delay}ms..." -ForegroundColor Yellow
        
        for ($i = 1; $i -le $count; $i++) {
            $testData = @{
                TestId = $i
                Message = "Pacote de teste #$i"
                Timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
            }
            
            Send-SimulatedEvent -EventType "TestEvent" -EventData $testData
            
            if ($i -lt $count) {
                Start-Sleep -Milliseconds $delay
            }
        }
        
        Write-Host "Envio de pacotes concluído!" -ForegroundColor Green
    }
    
    "4" {
        Write-Host "Modo interativo ativado. Digite 'quit' para sair." -ForegroundColor Yellow
        Write-Host "Digite mensagens para enviar como pacotes UDP:"
        Write-Host ""
        
        while ($true) {
            $input = Read-Host "> "
            
            if ([string]::IsNullOrEmpty($input) -or $input.ToLower() -eq "quit") {
                break
            }
            
            Send-UdpPacket -Message $input
        }
    }
    
    default {
        Write-Host "Escolha inválida. Enviando mensagem padrão..." -ForegroundColor Yellow
        Send-UdpPacket -Message "Teste padrão - $(Get-Date)"
    }
}

Write-Host ""
Write-Host "Teste concluído!" -ForegroundColor Green
Write-Host "Verifique os logs do AlbionOnlineSniffer para confirmar recebimento." -ForegroundColor Cyan 