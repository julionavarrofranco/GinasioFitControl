# Como Executar o Site TTF

## Pré-requisitos

### 1. Instalar o .NET 8.0 SDK

Se ainda não tens o .NET instalado:

1. **Descarrega o .NET 8.0 SDK:**
   - Vai a: https://dotnet.microsoft.com/download/dotnet/8.0
   - Descarrega o SDK (não apenas o Runtime)
   - Instala o ficheiro descarregado

2. **Verifica a instalação:**
   - Abre o PowerShell ou Command Prompt
   - Executa: `dotnet --version`
   - Deve mostrar algo como: `8.0.xxx`

## Executar o Site

### Opção 1: Usando o Terminal (PowerShell/CMD)

1. **Abre o PowerShell ou Command Prompt**

2. **Navega até à pasta do projeto:**
   ```powershell
   cd TTFWebsite
   ```

3. **Restaura as dependências (primeira vez apenas):**
   ```powershell
   dotnet restore
   ```

4. **Executa o projeto:**
   ```powershell
   dotnet run
   ```

5. **Abre o navegador:**
   - O terminal vai mostrar algo como:
     ```
     Now listening on: https://localhost:5001
     Now listening on: http://localhost:5000
     ```
   - Abre o navegador e vai a: **https://localhost:5001** ou **http://localhost:5000**

### Opção 2: Usando o Visual Studio

1. **Abre o Visual Studio 2022** (ou versão mais recente)

2. **Abre o projeto:**
   - File → Open → Project/Solution
   - Seleciona o ficheiro `TTFWebsite.csproj`

3. **Executa o projeto:**
   - Pressiona **F5** ou clica no botão "Run" (▶️)
   - O Visual Studio vai abrir o navegador automaticamente

### Opção 3: Usando o Visual Studio Code

1. **Abre o Visual Studio Code**

2. **Abre a pasta do projeto:**
   - File → Open Folder
   - Seleciona a pasta `TTFWebsite`

3. **Instala a extensão C#** (se ainda não tiveres)

4. **Executa o projeto:**
   - Abre o Terminal integrado (Ctrl + `)
   - Executa: `dotnet run`
   - Clica no link que aparece no terminal ou abre manualmente: `https://localhost:5001`

## Resolução de Problemas

### Erro: "dotnet não é reconhecido"
- Instala o .NET SDK (ver pré-requisitos acima)
- Reinicia o terminal após a instalação

### Erro de certificado SSL
- Se aparecer um aviso sobre certificado SSL, podes:
  - Clicar em "Avançar" ou "Continuar" no navegador
  - Ou usar apenas `http://localhost:5000` (sem HTTPS)

### Porta já em uso
- Se a porta 5000 ou 5001 estiver ocupada, podes mudar no ficheiro `Properties/launchSettings.json`

## Parar o Servidor

- No terminal onde está a correr, pressiona **Ctrl + C**

