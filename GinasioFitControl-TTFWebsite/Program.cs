using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using TTFWebsite.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied"; // Será redirecionado para ChangePassword se PrimeiraVez=true
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// Configurar autorização com policy para verificar se password foi alterada
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PasswordChanged", policy =>
        policy.Requirements.Add(new TTFWebsite.Authorization.PasswordChangedRequirement()));
});

// Registrar o handler de autorização (scoped porque precisa de IHttpContextAccessor)
builder.Services.AddScoped<IAuthorizationHandler, TTFWebsite.Authorization.PasswordChangedHandler>();

// Session para guardar dados do utilizador
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registrar HttpClient e ApiService
builder.Services.AddHttpClient<TTFWebsite.Services.IApiService, TTFWebsite.Services.ApiService>();
builder.Services.AddHttpContextAccessor(); // <-- Adiciona isto
builder.Services.AddSingleton<JwtService>();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:7267";
builder.Services.AddHttpClient("FitControlApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();

app.UseMiddleware<JwtValidationMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

