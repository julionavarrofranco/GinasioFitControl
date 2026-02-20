# TTF Website - Replicação em ASP.NET Core

Este projeto é uma replicação do site TTF (https://ttf.pt/) desenvolvido em ASP.NET Core MVC.

## Funcionalidades

- Hero section com call-to-action
- Seção de localização de ginásios
- Benefícios do ginásio
- Tipos de treino disponíveis
- Depoimentos de clientes
- Planos e preços
- Formulário de treino grátis
- Newsletter subscription
- Design responsivo
- Paleta de cores personalizada

## Tecnologias

- ASP.NET Core 8.0
- MVC (Model-View-Controller)
- HTML5, CSS3, JavaScript
- Design responsivo

## Como executar

1. Certifique-se de ter o .NET 8.0 SDK instalado
2. Navegue até à pasta do projeto
3. Execute:
   ```
   dotnet restore
   dotnet run
   ```
4. Abra o navegador em `https://localhost:5001` ou `http://localhost:5000`

## Estrutura do Projeto

```
TTFWebsite/
├── Controllers/
│   └── HomeController.cs
├── Models/
│   ├── Gym.cs
│   ├── Plan.cs
│   ├── Benefit.cs
│   └── Testimonial.cs
├── Views/
│   ├── Home/
│   │   └── Index.cshtml
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/
│   ├── css/
│   │   └── site.css
│   └── js/
│       └── site.js
└── Program.cs
```

## Paleta de Cores

- Fundo principal: `#1E1F29`
- Painel/Cartões: `#2C2E3B`
- Cor de destaque: `#02ACC3`
- Hover: `#0192A6`
- Texto primário: `#FFFFFF`
- Texto secundário: `#B0B3C2`
- Bordas: `#3C3E4A`
- Erro: `#E74C3C`
- Sucesso: `#27AE60`

