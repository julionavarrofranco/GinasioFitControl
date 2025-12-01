@echo off
echo ========================================
echo   FitControl API - Iniciando Backend
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
echo A iniciar o servidor API...
echo.
echo A API vai estar disponivel em:
echo   HTTP: http://localhost:5295
echo   HTTPS: https://localhost:7267
echo   Swagger: http://localhost:5295/swagger
echo.
echo Para parar o servidor, pressiona Ctrl + C
echo.

dotnet run

