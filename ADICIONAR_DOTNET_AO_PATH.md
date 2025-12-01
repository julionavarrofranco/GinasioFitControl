# Como Adicionar o .NET SDK ao PATH no Windows

## Opção 1: Instalação Automática (Recomendado)

Se o .NET SDK ainda não está instalado, a instalação oficial já adiciona automaticamente ao PATH. Basta:

1. **Baixar e instalar o .NET 8.0 SDK:**
   - Acesse: https://dotnet.microsoft.com/download/dotnet/8.0
   - Baixe o instalador para Windows (x64)
   - Execute o instalador e siga as instruções
   - O instalador adiciona automaticamente ao PATH

2. **Reiniciar o terminal/PowerShell** após a instalação

3. **Verificar a instalação:**
   ```powershell
   dotnet --version
   ```

## Opção 2: Adicionar Manualmente ao PATH

Se o .NET já está instalado mas não está no PATH:

### Método A: Via Interface Gráfica

1. **Abrir as Variáveis de Ambiente:**
   - Pressione `Win + R`
   - Digite: `sysdm.cpl` e pressione Enter
   - Vá na aba "Avançado"
   - Clique em "Variáveis de Ambiente"

2. **Adicionar ao PATH do Sistema:**
   - Na seção "Variáveis do sistema", encontre "Path"
   - Clique em "Editar"
   - Clique em "Novo"
   - Adicione: `C:\Program Files\dotnet`
   - Clique em "OK" em todas as janelas

3. **Reiniciar o terminal/PowerShell**

### Método B: Via PowerShell (Como Administrador)

Execute o PowerShell **como Administrador** e execute:

```powershell
# Verificar se o .NET está instalado
$dotnetPath = "C:\Program Files\dotnet"
if (Test-Path $dotnetPath) {
    # Adicionar ao PATH do sistema
    $currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
    if ($currentPath -notlike "*$dotnetPath*") {
        [Environment]::SetEnvironmentVariable("Path", "$currentPath;$dotnetPath", "Machine")
        Write-Host "✓ .NET SDK adicionado ao PATH do sistema" -ForegroundColor Green
        Write-Host "Reinicie o terminal para aplicar as alterações" -ForegroundColor Yellow
    } else {
        Write-Host "✓ .NET SDK já está no PATH" -ForegroundColor Green
    }
} else {
    Write-Host "✗ .NET SDK não encontrado em $dotnetPath" -ForegroundColor Red
    Write-Host "Por favor, instale o .NET SDK primeiro" -ForegroundColor Yellow
}
```

### Método C: Adicionar apenas para a Sessão Atual

Se não tiver permissões de administrador, pode adicionar temporariamente:

```powershell
# Adicionar ao PATH da sessão atual
$env:Path += ";C:\Program Files\dotnet"
```

**Nota:** Esta alteração só funciona na sessão atual do PowerShell.

## Verificar se Funcionou

Após adicionar ao PATH, execute:

```powershell
dotnet --version
```

Se mostrar a versão (ex: `8.0.xxx`), está funcionando!

## Locais Comuns de Instalação

O .NET SDK pode estar instalado em:
- `C:\Program Files\dotnet` (mais comum)
- `C:\Program Files (x86)\dotnet` (32-bit)
- `%LOCALAPPDATA%\Microsoft\dotnet` (instalação por usuário)

## Solução de Problemas

Se ainda não funcionar após adicionar ao PATH:

1. **Reinicie o computador** (às vezes necessário)
2. **Verifique se o caminho está correto:**
   ```powershell
   Test-Path "C:\Program Files\dotnet\dotnet.exe"
   ```
3. **Verifique o PATH atual:**
   ```powershell
   $env:Path -split ';' | Select-String "dotnet"
   ```

