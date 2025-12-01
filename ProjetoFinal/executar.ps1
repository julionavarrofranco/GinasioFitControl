# Script para executar o backend API
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  FitControl API - Iniciando Backend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verifica se o .NET está instalado
Write-Host "A verificar se o .NET SDK está instalado..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK encontrado: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK não encontrado!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Por favor, instala o .NET 8.0 SDK:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "Pressiona Enter para sair"
    exit 1
}

Write-Host ""
Write-Host "A restaurar dependências..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Erro ao restaurar dependências" -ForegroundColor Red
    Read-Host "Pressiona Enter para sair"
    exit 1
}

Write-Host "✓ Dependências restauradas" -ForegroundColor Green
Write-Host ""
Write-Host "A iniciar o servidor API..." -ForegroundColor Yellow
Write-Host ""
Write-Host "A API vai estar disponível em:" -ForegroundColor Cyan
Write-Host "  HTTP: http://localhost:5295" -ForegroundColor Cyan
Write-Host "  HTTPS: https://localhost:7267" -ForegroundColor Cyan
Write-Host "  Swagger: http://localhost:5295/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para parar o servidor, pressiona Ctrl + C" -ForegroundColor Yellow
Write-Host ""

# Executa o projeto
dotnet run

