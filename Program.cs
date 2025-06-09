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
// Banco de Dados
// ---------------------------
var connection = Environment.GetEnvironmentVariable("DB_CONNECTION");
if (string.IsNullOrEmpty(connection))
{
    throw new InvalidOperationException("DB_CONNECTION não encontrada nas variáveis de ambiente!");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connection));

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
// Configuração do CORS
// --------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
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
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// ---------------------------
// Pipeline HTTP
// ---------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCors();
app.Run();