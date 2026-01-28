# Resolução de Problemas - "localhost recusou estabelecer ligação"

## Problema: Não consegues aceder ao localhost

Este erro significa que o servidor não está a correr. Vamos resolver passo a passo:

## ✅ Passo 1: Verificar se o .NET está instalado

1. **Abre o PowerShell** (clica com botão direito → "Executar como administrador")

2. **Executa este comando:**
   ```powershell
   dotnet --version
   ```

3. **Se aparecer um erro:**
   - O .NET não está instalado
   - **Solução:** Descarrega e instala o .NET 8.0 SDK:
     - Vai a: https://dotnet.microsoft.com/download/dotnet/8.0
     - Descarrega o **SDK** (não apenas o Runtime)
     - Instala o ficheiro descarregado
     - **Reinicia o computador** após a instalação
     - Abre um novo PowerShell e verifica novamente com `dotnet --version`

## ✅ Passo 2: Encontrar a pasta do projeto

1. **Procura a pasta `TTFWebsite`** no teu computador
   - Pode estar em: `C:\Users\TeuNome\TTFWebsite`
   - Ou em: `C:\Projetos\TTFWebsite`
   - Ou noutro local onde guardaste os ficheiros

2. **Abre o PowerShell na pasta do projeto:**
   - Navega até à pasta no Explorador de Ficheiros
   - Clica com botão direito na pasta `TTFWebsite`
   - Seleciona "Abrir no Terminal" ou "Abrir no PowerShell"

## ✅ Passo 3: Executar o projeto

1. **No PowerShell, dentro da pasta TTFWebsite, executa:**
   ```powershell
   dotnet restore
   ```
   (Aguarda até terminar)

2. **Depois executa:**
   ```powershell
   dotnet run
   ```

3. **Deves ver algo como:**
   ```
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: https://localhost:5001
         Now listening on: http://localhost:5000
   ```

4. **Se aparecer esta mensagem, o servidor está a correr!**
   - Abre o navegador
   - Vai a: **https://localhost:5001** ou **http://localhost:5000**

## ⚠️ Problemas Comuns

### Erro: "A porta 5000/5001 já está em uso"

**Solução:**
1. Fecha outros programas que possam estar a usar essas portas
2. Ou muda a porta no ficheiro `Properties/launchSettings.json`

### Erro: "Certificado SSL inválido"

**Solução:**
- Usa `http://localhost:5000` em vez de `https://localhost:5001`
- Ou clica em "Avançar" no aviso do navegador

### Erro: "dotnet não é reconhecido"

**Solução:**
- Instala o .NET SDK (ver Passo 1)
- **Importante:** Reinicia o computador após instalar
- Abre um novo PowerShell

### O servidor inicia mas o navegador não abre automaticamente

**Solução:**
- Copia o URL que aparece no terminal (ex: `https://localhost:5001`)
- Cola no navegador e pressiona Enter

## 🔍 Verificar se o servidor está a correr

1. **Vê a janela do terminal onde executaste `dotnet run`**
   - Se ainda estiver aberta e mostrar "Now listening on...", o servidor está a correr
   - Se fechaste a janela, o servidor parou

2. **Para parar o servidor:**
   - Vai à janela do terminal
   - Pressiona **Ctrl + C**

3. **Para voltar a iniciar:**
   - Executa `dotnet run` novamente

## 📝 Checklist Rápido

- [ ] .NET 8.0 SDK instalado? (`dotnet --version` funciona?)
- [ ] Estás na pasta correta? (pasta `TTFWebsite`)
- [ ] Executaste `dotnet restore`?
- [ ] Executaste `dotnet run`?
- [ ] O terminal mostra "Now listening on..."?
- [ ] Tentaste abrir `http://localhost:5000` no navegador?

## 💡 Dica

Se continuares com problemas, tenta:
1. Fechar todos os terminais
2. Reiniciar o computador
3. Abrir um novo PowerShell
4. Navegar até à pasta do projeto
5. Executar `dotnet run` novamente

