# Usar a imagem oficial do SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar arquivos do projeto e restaurar dependências
COPY *.csproj ./
RUN dotnet restore

# Copiar tudo e publicar a aplicação
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Usar imagem runtime para rodar a aplicação (mais leve)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expor a porta da aplicação
EXPOSE 80

# Comando para rodar a API
ENTRYPOINT ["dotnet", "apisteamstats.dll"]
