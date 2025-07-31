# Listar adaptadores de rede disponíveis
Write-Host "=== Adaptadores de Rede Disponíveis ===" -ForegroundColor Green

try {
    $adapters = Get-NetAdapter | Where-Object { $_.Status -eq "Up" }
    
    Write-Host "Adaptadores ativos:" -ForegroundColor Yellow
    for ($i = 0; $i -lt $adapters.Count; $i++) {
        $adapter = $adapters[$i]
        Write-Host "$i. $($adapter.Name) - $($adapter.InterfaceDescription)" -ForegroundColor Cyan
        Write-Host "   Status: $($adapter.Status), Speed: $($adapter.LinkSpeed)" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "Para usar um adaptador específico, modifique o código do PacketCaptureService." -ForegroundColor Yellow
}
catch {
    Write-Host "Erro ao listar adaptadores: $($_.Exception.Message)" -ForegroundColor Red
} 