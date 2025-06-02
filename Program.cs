using APISTEAMSTATS.data;
using APISTEAMSTATS.repository;
using APISTEAMSTATS.services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona suporte a controllers
builder.Services.AddControllers();

// Conexão com banco de dados
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));

// Registra os serviços no container de serviços
builder.Services.AddScoped<GameListService>();
builder.Services.AddScoped<SteamSpyAcl>();
builder.Services.AddScoped<GameListRepository>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<WishListService>();
builder.Services.AddScoped<WishListRepository>();
builder.Services.AddScoped<DailyTaskService>();
builder.Services.AddScoped<EmailAcl>();

// Configura JWT Authentication
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

// Configura Swagger para ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Adiciona autenticação e autorização no pipeline
app.UseAuthentication();
app.UseAuthorization();

// Mapeia os controllers
app.MapControllers();

app.Run();
