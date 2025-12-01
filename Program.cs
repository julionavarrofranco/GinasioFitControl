// ============================================================================
// PROGRAMA PRINCIPAL - PONTO DE ENTRADA DA APLICAÇÃO WEB FITCONTROL
// Este ficheiro configura e inicializa a aplicação web ASP.NET Core
// ============================================================================

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

// Criar o construtor da aplicação web com os argumentos da linha de comandos
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURAÇÃO DOS SERVIÇOS
// ============================================================================

// Adicionar suporte para controladores MVC com vistas Razor
builder.Services.AddControllersWithViews();

// Configurar autenticação baseada em cookies
// Os cookies permitem manter a sessão do utilizador entre pedidos HTTP
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Caminho para a página de login quando o utilizador não está autenticado
        options.LoginPath = "/Account/Login";
        // Caminho para terminar sessão
        options.LogoutPath = "/Account/Logout";
        // Caminho quando o acesso é negado (permissões insuficientes)
        options.AccessDeniedPath = "/Account/AccessDenied";
        // Tempo de expiração do cookie de autenticação (30 dias)
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        // Renovar automaticamente o cookie quando o utilizador está ativo
        options.SlidingExpiration = true;
    });

// Adicionar serviços de autorização para controlar o acesso às páginas
builder.Services.AddAuthorization();

// Configurar sessão para armazenar dados temporários do utilizador
// A sessão é utilizada para guardar tokens JWT e outras informações
builder.Services.AddSession(options =>
{
    // Tempo limite de inatividade da sessão (30 minutos)
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    // Cookie apenas acessível via HTTP (não via JavaScript) - segurança
    options.Cookie.HttpOnly = true;
    // Cookie essencial para funcionamento da aplicação
    options.Cookie.IsEssential = true;
});

// ============================================================================
// REGISTO DOS SERVIÇOS PERSONALIZADOS
// ============================================================================

// Registar o HttpClient e ApiService para comunicação com a API do backend
builder.Services.AddHttpClient<TTFWebsite.Services.IApiService, TTFWebsite.Services.ApiService>();
// Registar o ApiService com âmbito (scope) - nova instância por pedido HTTP
builder.Services.AddScoped<TTFWebsite.Services.IApiService, TTFWebsite.Services.ApiService>();
// Registar o serviço JWT para descodificação de tokens
builder.Services.AddScoped<TTFWebsite.Services.JwtService>();

// Obter o URL base da API a partir das configurações (appsettings.json)
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5295";
// Registar um HttpClient nomeado para comunicação com a API FitControl
builder.Services.AddHttpClient("FitControlApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Construir a aplicação com todas as configurações
var app = builder.Build();

// ============================================================================
// CONFIGURAÇÃO DO PIPELINE DE PEDIDOS HTTP
// ============================================================================

// Configuração específica para ambiente de produção
if (!app.Environment.IsDevelopment())
{
    // Página de erro amigável para exceções não tratadas
    app.UseExceptionHandler("/Home/Error");
    // Forçar utilização de HTTPS (HTTP Strict Transport Security)
    app.UseHsts();
}

// Redirecionar pedidos HTTP para HTTPS
app.UseHttpsRedirection();
// Permitir servir ficheiros estáticos (CSS, JavaScript, imagens)
app.UseStaticFiles();

// Configurar o sistema de encaminhamento de rotas
app.UseRouting();

// Ativar middleware de sessão (deve estar antes da autenticação)
app.UseSession();
// Ativar middleware de autenticação
app.UseAuthentication();
// Ativar middleware de autorização
app.UseAuthorization();

// Configurar a rota padrão da aplicação MVC
// Formato: /{controlador}/{ação}/{id opcional}
// Por defeito: HomeController.Index()
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Iniciar a aplicação e começar a escutar pedidos
app.Run();
