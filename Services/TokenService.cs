using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using APISTEAMSTATS.models;

public class TokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public TokenService()
    {
        // Busca as variáveis de ambiente diretamente
        _secretKey = Environment.GetEnvironmentVariable("JWT_KEY") 
            ?? throw new InvalidOperationException("JWT_KEY não encontrada nas variáveis de ambiente!");
        
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
            ?? throw new InvalidOperationException("JWT_ISSUER não encontrada nas variáveis de ambiente!");
        
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
            ?? throw new InvalidOperationException("JWT_AUDIENCE não encontrada nas variáveis de ambiente!");
        
        _expirationMinutes = 1440; // 24 horas
    }

    public string GenerateToken(User user)
    {
        // 1. Cria a chave
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

        // 2. Cria as credenciais (com algoritmo de hash)
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Define as claims (informações do usuário)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Login)
        };

        // 4. Cria o token usando as variáveis de ambiente
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        // 5. Retorna o token como string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}