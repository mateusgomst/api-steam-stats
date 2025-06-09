using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class EmailAcl
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiToken;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailAcl()
    {
        // Carrega as configurações APENAS das variáveis de ambiente
        _apiUrl = Environment.GetEnvironmentVariable("MAILERSEND_API_URL");
        _apiToken = Environment.GetEnvironmentVariable("MAILERSEND_API_TOKEN");
        _fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS");
        _fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? "Steam Stats Notifier";

        // Valida se as configurações obrigatórias estão presentes
        if (string.IsNullOrEmpty(_apiUrl))
        {
            Console.WriteLine("ERRO: MAILERSEND_API_URL não foi configurada no .env");
            return;
        }
        
        if (string.IsNullOrEmpty(_apiToken))
        {
            Console.WriteLine("ERRO: MAILERSEND_API_TOKEN não foi configurada no .env");
            return;
        }
        
        if (string.IsNullOrEmpty(_fromEmail))
        {
            Console.WriteLine("ERRO: EMAIL_FROM_ADDRESS não foi configurada no .env");
            return;
        }

        // Configura o HttpClient
        _httpClient = new HttpClient();
        
        // Configura timeout se especificado
        if (int.TryParse(Environment.GetEnvironmentVariable("EMAIL_TIMEOUT_SECONDS"), out int timeoutSeconds))
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }
        
        _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
    }

    public async Task<(bool Success, string ErrorMessage)> SendPromotionEmail(string toEmail, string gameName, int discount, int appid)
    {
        var emailContent = new
        {
            from = new
            {
                email = _fromEmail,
                name = _fromName
            },
            to = new[]
            {
                new
                {
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
                <a href='https://store.steampowered.com/app/{appid}' target='_blank' style='display: inline-block; margin-top: 20px; padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Ver jogo na Steam</a>
                <p style='font-size: 12px; color: #999; margin-top: 30px;'>Você está recebendo este e-mail porque adicionou este jogo à sua lista de desejos no Steam Stats.</p>
            </div>"
        };

        try
        {
            string json = JsonSerializer.Serialize(emailContent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                string errorMsg = $"Falha na API de email - Status: {response.StatusCode}, Resposta: {responseContent}";
                return (false, errorMsg);
            }

            return (true, string.Empty);
        }
        catch (HttpRequestException ex)
        {
            string errorMsg = $"Erro de conexão com API de email: {ex.Message}";
            return (false, errorMsg);
        }
        catch (TaskCanceledException ex)
        {
            string errorMsg = $"Timeout na API de email: {ex.Message}";
            return (false, errorMsg);
        }
        catch (Exception ex)
        {
            // Lança exceção para erros inesperados
            throw new InvalidOperationException($"Erro inesperado na API de email: {ex.Message}", ex);
        }
    }

    // Implementa IDisposable para liberar recursos do HttpClient
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}