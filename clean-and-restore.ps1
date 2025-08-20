# Script para limpar e restaurar o projeto AlbionOnlineSniffer
# Execute este script no PowerShell como administrador

Write-Host "🧹 Limpando projeto AlbionOnlineSniffer..." -ForegroundColor Yellow

# Para todos os processos do .NET que possam estar rodando
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "AlbionOnlineSniffer.Web" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Limpa diretórios de build
Write-Host "🗑️ Removendo diretórios de build..." -ForegroundColor Cyan
Remove-Item -Path "src\**\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\**\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue

# Limpa cache do NuGet
Write-Host "📦 Limpando cache do NuGet..." -ForegroundColor Cyan
dotnet nuget locals all --clear

# Restaura dependências
Write-Host "📥 Restaurando dependências..." -ForegroundColor Green
dotnet restore

# Compila o projeto
Write-Host "🔨 Compilando projeto..." -ForegroundColor Green
dotnet build --no-restore

Write-Host "✅ Limpeza e restauração concluídas!" -ForegroundColor Green
Write-Host "🚀 Agora tente executar o projeto novamente no Rider" -ForegroundColor Green
