using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class EmailAcl
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://api.mailersend.com/v1/email";
    private const string ApiToken = "mlsn.918f8c0e05c515ccd8b4c3ef6c09e9413fc4e0aae8b7d5296f0112f1221f639b";

    public EmailAcl()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiToken);
    }

    public async Task SendPromotionEmail(string toEmail, string gameName, int discount)
    {
        var emailContent = new
        {
            from = new {
                email = "steamstats@test-2p0347z82r7lzdrn.mlsender.net",
                name = "Steam Stats Notifier"
            },
            to = new[] {
                new {
                    email = toEmail,
                    name = "Usuário"
                }
            },
            subject = "Um jogo da sua Wish List entrou de promoção!",
            text = $"O jogo \"{gameName}\" entrou em promoção com desconto de {discount}%. Aproveite!",
            html = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                <h2 style='color: #333;'>🎮 Promoção na sua Lista de Desejos!</h2>
                <p style='font-size: 16px; color: #555;'>O jogo <strong>{gameName}</strong> está com um super desconto de <strong style='color: #28a745;'>{discount}%</strong>!</p>
                <p style='font-size: 15px; color: #555;'>Aproveite a oferta antes que acabe!</p>
                <a href='https://store.steampowered.com/app/{gameName}' target='_blank' style='display: inline-block; margin-top: 20px; padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Ver jogo na Steam</a>
                <p style='font-size: 12px; color: #999; margin-top: 30px;'>Você está recebendo este e-mail porque adicionou este jogo à sua lista de desejos no Steam Stats.</p>
            </div>"

        };

        string json = JsonSerializer.Serialize(emailContent);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Erro ao enviar email: {response.StatusCode} - {responseContent}");
        }
    }


    // Importante: Implementar IDisposable para liberar recursos do HttpClient
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}