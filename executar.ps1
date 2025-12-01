# Script para executar o site TTF
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TTF Website - Iniciando Servidor" -ForegroundColor Cyan
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
Write-Host "A iniciar o servidor..." -ForegroundColor Yellow
Write-Host ""
Write-Host "O site vai abrir automaticamente no navegador." -ForegroundColor Cyan
Write-Host "URL: https://localhost:5001 ou http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para parar o servidor, pressiona Ctrl + C" -ForegroundColor Yellow
Write-Host ""

# Executa o projeto
dotnet run

