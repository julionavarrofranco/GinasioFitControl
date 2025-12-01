# Instalar .NET 8.0 Runtime

## Problema
O projeto precisa do **.NET 8.0 Runtime** para executar, mas você só tem o SDK 10.0 instalado.

## Solução: Instalar .NET 8.0 Runtime

### Opção 1: Download Direto (Recomendado)

1. **Acesse o link direto:**
   - https://dotnet.microsoft.com/download/dotnet/8.0
   - Ou diretamente: https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.11-windows-x64-installer

2. **Baixe o "ASP.NET Core Runtime 8.0.x"** (não o SDK)
   - Escolha a versão **x64** para Windows
   - O instalador tem cerca de 100-200 MB

3. **Execute o instalador** e siga as instruções

4. **Reinicie o terminal/PowerShell** após a instalação

5. **Teste novamente:**
   ```powershell
   cd C:\GinasioFitControl-main\ProjetoFinal
   dotnet run
   ```

### Opção 2: Via .NET Installer (Instala Tudo)

1. **Baixe o .NET 8.0 SDK completo:**
   - https://dotnet.microsoft.com/download/dotnet/8.0
   - Baixe o **.NET 8.0 SDK** (inclui o runtime)

2. **Execute o instalador**

3. **Reinicie o terminal**

### Verificar Instalação

Após instalar, verifique se o runtime 8.0 está disponível:

```powershell
dotnet --list-runtimes
```

Você deve ver algo como:
```
Microsoft.AspNetCore.App 8.0.x [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
Microsoft.NETCore.App 8.0.x [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
```

## Alternativa: Atualizar Projeto para .NET 10.0

Se preferir usar apenas .NET 10.0, posso atualizar o projeto para usar .NET 10.0 em vez de 8.0. Isso requer alterar o arquivo `.csproj`.

**Qual opção prefere?**
- ✅ Instalar .NET 8.0 Runtime (recomendado - mantém compatibilidade)
- 🔄 Atualizar projeto para .NET 10.0 (mais moderno, mas pode precisar ajustes)

