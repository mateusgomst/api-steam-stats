using APISTEAMSTATS.data;
using APISTEAMSTATS.services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona suporte a controllers
builder.Services.AddControllers();

// Conexão com banco de dados
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));

// **Registra o GameListService no container de serviços**
builder.Services.AddScoped<GameListService>();

var app = builder.Build();

// Configura Swagger para ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Mapeia os controllers
app.MapControllers();

app.Run();
