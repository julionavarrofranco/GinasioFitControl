# Script para adicionar .NET SDK ao PATH
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Adicionar .NET SDK ao PATH" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se está executando como Administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "⚠ ATENÇÃO: Este script precisa ser executado como Administrador" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Para adicionar ao PATH do sistema:" -ForegroundColor Yellow
    Write-Host "1. Clique com botão direito no PowerShell" -ForegroundColor Cyan
    Write-Host "2. Selecione 'Executar como Administrador'" -ForegroundColor Cyan
    Write-Host "3. Execute este script novamente" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Ou adicione temporariamente para esta sessão:" -ForegroundColor Yellow
    Write-Host '$env:Path += ";C:\Program Files\dotnet"' -ForegroundColor Cyan
    Write-Host ""
    Read-Host "Pressiona Enter para continuar (adicionar apenas para esta sessão)"
    
    # Adicionar temporariamente para esta sessão
    $dotnetPath = "C:\Program Files\dotnet"
    if (Test-Path $dotnetPath) {
        $env:Path += ";$dotnetPath"
        Write-Host "✓ .NET SDK adicionado ao PATH desta sessão" -ForegroundColor Green
        Write-Host "Testando..." -ForegroundColor Yellow
        $version = & "$dotnetPath\dotnet.exe" --version 2>$null
        if ($version) {
            Write-Host "✓ .NET encontrado: $version" -ForegroundColor Green
        }
    } else {
        Write-Host "✗ .NET SDK não encontrado em $dotnetPath" -ForegroundColor Red
        Write-Host ""
        Write-Host "Por favor, instale o .NET 8.0 SDK primeiro:" -ForegroundColor Yellow
        Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    }
    exit
}

# Se chegou aqui, está como Administrador
Write-Host "✓ Executando como Administrador" -ForegroundColor Green
Write-Host ""

# Locais comuns onde o .NET pode estar instalado
$possiblePaths = @(
    "C:\Program Files\dotnet",
    "C:\Program Files (x86)\dotnet",
    "$env:LOCALAPPDATA\Microsoft\dotnet"
)

$dotnetPath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path "$path\dotnet.exe") {
        $dotnetPath = $path
        Write-Host "✓ .NET SDK encontrado em: $dotnetPath" -ForegroundColor Green
        break
    }
}

if (-not $dotnetPath) {
    Write-Host "✗ .NET SDK não encontrado!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Por favor, instale o .NET 8.0 SDK primeiro:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "Pressiona Enter para sair"
    exit 1
}

# Verificar se já está no PATH
$currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
if ($currentPath -like "*$dotnetPath*") {
    Write-Host "✓ .NET SDK já está no PATH do sistema" -ForegroundColor Green
} else {
    # Adicionar ao PATH do sistema
    Write-Host "A adicionar ao PATH do sistema..." -ForegroundColor Yellow
    [Environment]::SetEnvironmentVariable("Path", "$currentPath;$dotnetPath", "Machine")
    Write-Host "✓ .NET SDK adicionado ao PATH do sistema" -ForegroundColor Green
}

# Também adicionar à sessão atual
$env:Path += ";$dotnetPath"

# Verificar se funciona
Write-Host ""
Write-Host "A verificar instalação..." -ForegroundColor Yellow
$version = & "$dotnetPath\dotnet.exe" --version
if ($version) {
    Write-Host "✓ .NET SDK funcionando: $version" -ForegroundColor Green
    Write-Host ""
    Write-Host "IMPORTANTE: Reinicie o terminal/PowerShell para aplicar as alterações permanentemente" -ForegroundColor Yellow
} else {
    Write-Host "✗ Erro ao verificar versão do .NET" -ForegroundColor Red
}

Write-Host ""
Read-Host "Pressiona Enter para sair"

