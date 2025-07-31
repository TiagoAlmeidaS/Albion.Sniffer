# Script para compilar e executar o TestPacketSender
# Este script compila o programa de teste e o executa

Write-Host "=== Compilando TestPacketSender ===" -ForegroundColor Green

try {
    # Verificar se o arquivo .csproj existe
    if (-not (Test-Path "TestPacketSender.csproj")) {
        Write-Host "✗ Arquivo TestPacketSender.csproj não encontrado!" -ForegroundColor Red
        Write-Host "Certifique-se de estar no diretório correto." -ForegroundColor Yellow
        exit 1
    }

    # Limpar builds anteriores
    Write-Host "Limpando builds anteriores..." -ForegroundColor Yellow
    dotnet clean TestPacketSender.csproj

    # Restaurar dependências
    Write-Host "Restaurando dependências..." -ForegroundColor Yellow
    dotnet restore TestPacketSender.csproj

    # Compilar o projeto
    Write-Host "Compilando projeto..." -ForegroundColor Yellow
    dotnet build TestPacketSender.csproj --configuration Release

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Compilação concluída com sucesso!" -ForegroundColor Green
        
        # Executar o programa
        Write-Host ""
        Write-Host "=== Executando TestPacketSender ===" -ForegroundColor Green
        Write-Host "Certifique-se de que o AlbionOnlineSniffer está rodando primeiro!" -ForegroundColor Yellow
        Write-Host ""
        
        dotnet run --project TestPacketSender.csproj
    }
    else {
        Write-Host "✗ Erro na compilação!" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "✗ Erro durante o processo: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} 