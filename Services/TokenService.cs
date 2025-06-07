using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using APISTEAMSTATS.models;

public class TokenService
{
    private readonly string _secretKey;
    private readonly int _expirationMinutes;

    public TokenService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:Key"];
        _expirationMinutes = 1440;
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
            new Claim(JwtRegisteredClaimNames.UniqueName, user.login)
        };

        // 4. Cria o token
        var token = new JwtSecurityToken(
            issuer: "apisteamstats",              // quem emitiu
            audience: "apisteamstats_client",     // quem consome
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        // 5. Retorna o token como string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}