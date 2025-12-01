@echo off
echo ========================================
echo   TTF Website - Iniciando Servidor
echo ========================================
echo.

echo A verificar se o .NET SDK esta instalado...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERRO] .NET SDK nao encontrado!
    echo.
    echo Por favor, instala o .NET 8.0 SDK:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo [OK] .NET SDK encontrado
echo.
echo A restaurar dependencias...
dotnet restore

if %errorlevel% neq 0 (
    echo [ERRO] Erro ao restaurar dependencias
    pause
    exit /b 1
)

echo [OK] Dependencias restauradas
echo.
echo A iniciar o servidor...
echo.
echo O site vai abrir automaticamente no navegador.
echo URL: https://localhost:5001 ou http://localhost:5000
echo.
echo Para parar o servidor, pressiona Ctrl + C
echo.

dotnet run

