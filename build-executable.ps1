# Script para compilar e publicar o executável do Albion Online Sniffer
# Autor: Albion Sniffer Project
# Data: 2024

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$Clean,
    [switch]$Publish
)

Write-Host "🔧 Albion Online Sniffer - Build Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Configurações
$ProjectPath = "src\AlbionOnlineSniffer.App\AlbionOnlineSniffer.App.csproj"
$PublishPath = "dist\AlbionOnlineSniffer-Published"

# Limpar se solicitado
if ($Clean) {
    Write-Host "🧹 Limpando builds anteriores..." -ForegroundColor Yellow
    if (Test-Path $PublishPath) {
        Remove-Item -Path $PublishPath -Recurse -Force
    }
    dotnet clean $ProjectPath
    Write-Host "✅ Limpeza concluída!" -ForegroundColor Green
}

# Restaurar dependências
Write-Host "📦 Restaurando dependências..." -ForegroundColor Yellow
dotnet restore $ProjectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao restaurar dependências!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Dependências restauradas!" -ForegroundColor Green

# Compilar projeto
Write-Host "🔨 Compilando projeto ($Configuration)..." -ForegroundColor Yellow
dotnet build $ProjectPath -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro na compilação!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Compilação concluída!" -ForegroundColor Green

# Publicar se solicitado
if ($Publish) {
    Write-Host "📤 Publicando executável ($Runtime)..." -ForegroundColor Yellow
    
    # Criar diretório de saída
    if (!(Test-Path $PublishPath)) {
        New-Item -ItemType Directory -Path $PublishPath -Force | Out-Null
    }
    
    # Publicar como arquivo único
    dotnet publish $ProjectPath -c $Configuration -r $Runtime --self-contained true --output $PublishPath -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishTrimmed=true --no-restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Erro na publicação!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Publicação concluída!" -ForegroundColor Green
    Write-Host "📁 Executável disponível em: $PublishPath" -ForegroundColor Cyan
    
    # Mostrar informações do executável
    $exePath = Join-Path $PublishPath "AlbionOnlineSniffer.exe"
    if (Test-Path $exePath) {
        $fileInfo = Get-Item $exePath
        Write-Host "📊 Informações do executável:" -ForegroundColor Cyan
        Write-Host "   Nome: $($fileInfo.Name)" -ForegroundColor White
        Write-Host "   Tamanho: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor White
        Write-Host "   Data: $($fileInfo.CreationTime)" -ForegroundColor White
    }
}

# Executar se não for apenas publicação
if (!$Publish) {
    Write-Host "🚀 Executando aplicação..." -ForegroundColor Yellow
    dotnet run --project $ProjectPath -c $Configuration --no-build
}

Write-Host "🎉 Processo concluído!" -ForegroundColor Green 