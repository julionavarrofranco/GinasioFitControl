using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using TTFWebsite.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Rotas públicas do fluxo de autenticação (não alterar sem ajustar controllers/views).
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        // AccessDenied redireciona para ChangePassword quando NeedsPasswordChange=True.
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PasswordChanged", policy =>
        policy.Requirements.Add(new TTFWebsite.Authorization.PasswordChangedRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, TTFWebsite.Authorization.PasswordChangedHandler>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient<TTFWebsite.Services.IApiService, TTFWebsite.Services.ApiService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<JwtService>();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:7267";
builder.Services.AddHttpClient("FitControlApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

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

// Rota default (não alterar: é a base das rotas MVC do site)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

