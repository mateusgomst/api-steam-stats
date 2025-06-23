using APISTEAMSTATS.data;
using APISTEAMSTATS.repository;
using APISTEAMSTATS.services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------------
// Carregar variáveis do .env
// --------------------------
DotNetEnv.Env.Load(); 

// IMPORTANTE: Adicionar as variáveis de ambiente à configuração
builder.Configuration.AddEnvironmentVariables();

// --------------------------
// Serviços padrão do seu app
// --------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// ---------------------------
// Banco de Dados - Neon PostgreSQL
// ---------------------------
var connection = Environment.GetEnvironmentVariable("DB_CONNECTION");
if (string.IsNullOrEmpty(connection))
{
    throw new InvalidOperationException("DB_CONNECTION não encontrada nas variáveis de ambiente!");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connection, npgsqlOptions =>
    {
        // Configurações específicas para Neon
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        
        // Timeout para comandos
        npgsqlOptions.CommandTimeout(30);
    });
    
    // Habilitar logging sensível apenas em desenvolvimento
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// ---------------------------
// Registro de dependências
// ---------------------------
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<SteamSpyAcl>();
builder.Services.AddScoped<GameRepository>();
builder.Services.AddSingleton<TokenService>(); // Singleton é melhor para TokenService
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<WishGameService>();
builder.Services.AddScoped<WishGameRepository>();
builder.Services.AddScoped<DailyTaskService>();
builder.Services.AddScoped<EmailAcl>();

// ---------------------------
// Configuração do Quartz.NET
// ---------------------------
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DailyJob");

    q.AddJob<DailyJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DailyJob-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(1440) // 24 horas
            .RepeatForever())
    );
});

// --------------------------
// Configuração do CORS - CORRIGIDA
// --------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://steamstats-silk.vercel.app", // Seu domínio específico
                "https://steamstats-*.vercel.app" // Pattern para outros subdomínios
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains(); // Permite wildcards
    });
    
    // Política adicional mais permissiva para desenvolvimento/teste
    options.AddPolicy("AllowVercel", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                
                // Permite localhost para desenvolvimento
                if (origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost"))
                    return true;
                
                // Permite qualquer subdomínio do Vercel
                if (origin.EndsWith(".vercel.app"))
                    return true;
                
                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// ---------------------------
// JWT Authentication
// ---------------------------
var secretKey = Environment.GetEnvironmentVariable("JWT_KEY");
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

// Validar se as variáveis JWT existem
if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("JWT_KEY não encontrada nas variáveis de ambiente!");
if (string.IsNullOrEmpty(issuer))
    throw new InvalidOperationException("JWT_ISSUER não encontrada nas variáveis de ambiente!");
if (string.IsNullOrEmpty(audience))
    throw new InvalidOperationException("JWT_AUDIENCE não encontrada nas variáveis de ambiente!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = false,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// ---------------------------
// Aplicar migrações automaticamente (apenas em desenvolvimento)
// ---------------------------
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Testar conexão
            await context.Database.CanConnectAsync();
            Console.WriteLine("✅ Conexão com banco Neon estabelecida com sucesso!");
            
            // Aplicar migrações pendentes
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Console.WriteLine($"📦 Aplicando {pendingMigrations.Count()} migração(ões)...");
                await context.Database.MigrateAsync();
                Console.WriteLine("✅ Migrações aplicadas com sucesso!");
            }
            else
            {
                Console.WriteLine("✅ Banco de dados está atualizado!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao conectar/migrar banco: {ex.Message}");
            // Em desenvolvimento, podemos continuar para debugar
            // Em produção, você pode querer fazer throw aqui
        }
    }
}

// ---------------------------
// Pipeline HTTP - ORDEM CORRIGIDA
// ---------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANTE: CORS deve ser uma das primeiras coisas no pipeline
app.UseCors("AllowVercel"); // Usando a política mais permissiva

// Middleware de roteamento
app.UseRouting();

// Descomente apenas se estiver usando HTTPS
// app.UseHttpsRedirection();

// Autenticação e autorização vêm DEPOIS do CORS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---------------------------
// Health Check Endpoint
// ---------------------------
app.MapGet("/health", async (AppDbContext context) =>
{
    try
    {
        await context.Database.CanConnectAsync();
        return Results.Ok(new { 
            status = "OK", 
            database = "Connected",
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName,
            cors = "Enabled for Vercel"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

// Endpoint para testar CORS
app.MapGet("/cors-test", () =>
{
    return Results.Ok(new { message = "CORS is working!", timestamp = DateTime.UtcNow });
});

app.Run();