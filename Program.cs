using APISTEAMSTATS.data;
using APISTEAMSTATS.repository;
using APISTEAMSTATS.services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz; // ⬅️ Adicionado para agendamento com Quartz
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------------
// Serviços padrão do seu app
// --------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));

// ---------------------------
// Registro de dependências
// ---------------------------
builder.Services.AddScoped<GameListService>();
builder.Services.AddScoped<SteamSpyAcl>();
builder.Services.AddScoped<GameListRepository>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<WishListService>();
builder.Services.AddScoped<WishListRepository>();
builder.Services.AddScoped<DailyTaskService>();     // Serviço usado na job
builder.Services.AddScoped<EmailAcl>();

// ---------------------------
// Configuração do Quartz.NET
// ---------------------------
builder.Services.AddQuartz(q =>
{
    // Define a identidade da job
    var jobKey = new JobKey("DailyJob");

    // Registra a job no Quartz (DailyJob precisa implementar IJob)
    q.AddJob<DailyJob>(opts => opts.WithIdentity(jobKey));

    // Registra o gatilho da job — executa a cada 1 minuto (apenas para testes)
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DailyJob-trigger")
        .StartNow() // Começa imediatamente
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(1440) // 🔁 Altere para 1440 (24h) depois dos testes
            .RepeatForever())
    );
});

// Ativa o agendador como serviço hospedado
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// ---------------------------
// JWT Authentication
// ---------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
string secretKey = jwtSettings["Key"];
string issuer = jwtSettings["Issuer"];
string audience = jwtSettings["Audience"];

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

app.Run();
