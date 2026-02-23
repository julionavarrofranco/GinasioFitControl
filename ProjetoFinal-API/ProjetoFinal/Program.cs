using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjetoFinal;
using ProjetoFinal.Data;
using ProjetoFinal.Services;
using ProjetoFinal.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FitControl API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    };
    c.AddSecurityRequirement(securityRequirement);
});

// Configuração de serviços e dependências
builder.Services.AddDbContext<GinasioDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPhysicalEvaluationService, PhysicalEvaluationService>();
builder.Services.AddScoped<IPhysicalEvaluationReservationService, PhysicalEvaluationReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IScheduleClassService, ScheduleClassService>();
builder.Services.AddScoped<IMemberClassService, MemberClassService>();
builder.Services.AddScoped<ITrainingPlanService, TrainingPlanService>();
builder.Services.AddScoped<IExercisePlanService, ExercisePlanService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Configuração de autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
    AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"]!)),
            ValidateIssuerSigningKey = true
        };
    }
);

// Configuração de políticas de autorização

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireClaim("Funcao", "Admin", "Rececao"));

    options.AddPolicy("CanViewMembers", policy =>
        policy.RequireClaim("Funcao", "Admin", "Rececao", "PT"));

    options.AddPolicy("CanManagePayments", policy =>
        policy.RequireClaim("Funcao", "Admin", "Rececao"));

    options.AddPolicy("OnlyAdmin", policy =>
        policy.RequireClaim("Funcao", "Admin"));

    options.AddPolicy("CanViewExercises", policy =>
        policy.RequireClaim("Funcao", "Admin", "PT"));

    options.AddPolicy("CanViewSubscriptions", policy =>
        policy.RequireClaim("Funcao", "Admin", "Rececao"));

    options.AddPolicy("OnlyPT", policy =>
        policy.RequireClaim("Funcao", "PT"));

    options.AddPolicy("OnlyMembers", policy =>
        policy.RequireClaim("Tipo", "Membro"));

    options.AddPolicy("CanViewClasses", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(c =>
            (c.Type == "Funcao" && (c.Value == "Admin" || c.Value == "PT")) ||
            (c.Type == "Tipo" && c.Value == "Membro")
        )
    ));

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        // Aplica migrations e faz seed
        var context = services.GetRequiredService<GinasioDbContext>();
        await context.Database.MigrateAsync();

        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao seedar a base de dados");
    }
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

