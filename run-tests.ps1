# Script para executar testes no Windows
param(
    [switch]$Unit,
    [switch]$Integration,
    [switch]$E2E,
    [switch]$Coverage,
    [switch]$Watch,
    [string]$Filter = "",
    [switch]$Help
)

Write-Host "🧪 Albion Online Sniffer - Test Runner" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

if ($Help) {
    Write-Host "Uso: .\run-tests.ps1 [opções]"
    Write-Host ""
    Write-Host "Opções:"
    Write-Host "  -Unit          Executa apenas testes unitários"
    Write-Host "  -Integration   Executa apenas testes de integração"
    Write-Host "  -E2E           Executa apenas testes E2E"
    Write-Host "  -Coverage      Gera relatório de cobertura"
    Write-Host "  -Watch         Executa em modo watch"
    Write-Host "  -Filter <exp>  Aplica filtro customizado"
    Write-Host "  -Help          Mostra esta ajuda"
    Write-Host ""
    Write-Host "Exemplos:"
    Write-Host "  .\run-tests.ps1 -Unit                    # Executa apenas testes unitários"
    Write-Host "  .\run-tests.ps1 -Integration -Coverage   # Testes de integração com cobertura"
    Write-Host '  .\run-tests.ps1 -Filter "FullyQualifiedName~PhotonParser"  # Testes específicos'
    exit 0
}

# Determina o tipo de teste
$testType = "all"
$testFilter = $Filter

if ($Unit) {
    $testType = "unit"
    $testFilter = "Category!=Integration&Category!=E2E"
}
elseif ($Integration) {
    $testType = "integration"
    $testFilter = "Category=Integration"
}
elseif ($E2E) {
    $testType = "e2e"
    $testFilter = "Category=E2E"
}

# Restaura dependências
Write-Host "📦 Restaurando dependências..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao restaurar dependências!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Build
Write-Host "🔨 Compilando projeto..." -ForegroundColor Yellow
dotnet build --no-restore --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao compilar projeto!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Prepara comando de teste
$testCmd = @()

if ($Watch) {
    $testCmd += "watch"
}
$testCmd += "test"
$testCmd += "--no-build"
$testCmd += "--configuration"
$testCmd += "Release"

if ($testFilter) {
    $testCmd += "--filter"
    $testCmd += $testFilter
}

$testCmd += "--logger"
$testCmd += "console;verbosity=normal"

if ($Coverage) {
    $testCmd += "--collect:`"XPlat Code Coverage`""
    $testCmd += "--results-directory"
    $testCmd += "./TestResults"
    $testCmd += "--"
    $testCmd += "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover"
}

# Executa testes
Write-Host ""
Write-Host "🚀 Executando testes ($testType)..." -ForegroundColor Yellow
Write-Host "Comando: dotnet $($testCmd -join ' ')" -ForegroundColor Gray
Write-Host ""

& dotnet $testCmd
$testResult = $LASTEXITCODE

# Gera relatório de cobertura se solicitado
if ($Coverage -and $testResult -eq 0) {
    Write-Host ""
    Write-Host "📊 Gerando relatório de cobertura..." -ForegroundColor Yellow
    
    # Verifica se ReportGenerator está instalado
    $reportGeneratorInstalled = $false
    try {
        reportgenerator --version | Out-Null
        $reportGeneratorInstalled = $true
    }
    catch {
        $reportGeneratorInstalled = $false
    }
    
    if (-not $reportGeneratorInstalled) {
        Write-Host "Instalando ReportGenerator..."
        dotnet tool install --global dotnet-reportgenerator-globaltool
    }
    
    # Gera relatório HTML
    reportgenerator `
        -reports:./TestResults/**/coverage.opencover.xml `
        -targetdir:./CoverageReport `
        -reporttypes:"Html;Cobertura;TextSummary"
    
    Write-Host ""
    Write-Host "✅ Relatório de cobertura gerado em: ./CoverageReport/index.html" -ForegroundColor Green
    
    # Mostra resumo no console
    $summaryFile = "./CoverageReport/Summary.txt"
    if (Test-Path $summaryFile) {
        Write-Host ""
        Write-Host "📈 Resumo da Cobertura:" -ForegroundColor Cyan
        Get-Content $summaryFile
    }
    
    # Abre o relatório no navegador
    $reportFile = Join-Path (Get-Location) "CoverageReport\index.html"
    if (Test-Path $reportFile) {
        Write-Host ""
        Write-Host "Abrindo relatório no navegador..." -ForegroundColor Gray
        Start-Process $reportFile
    }
}

# Resultado final
Write-Host ""
if ($testResult -eq 0) {
    Write-Host "✅ Testes executados com sucesso!" -ForegroundColor Green
}
else {
    Write-Host "❌ Alguns testes falharam!" -ForegroundColor Red
    exit $testResult
}