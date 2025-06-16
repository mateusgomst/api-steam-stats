using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace APISTEAMSTATS.services
{
    public class SteamSpyAcl : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _urlGetAllGames;
        private bool _disposed = false;

        public SteamSpyAcl()
        {
            // Configura HttpClientHandler com tratamento SSL personalizado
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = ValidateServerCertificate
            };

            _httpClient = new HttpClient(handler);

            // Configurações de timeout e headers
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // Aumenta timeout para APIs lentas
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SteamStats/1.0 (Steam promotion tracker)");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _urlGetAllGames = Environment.GetEnvironmentVariable("STEAMSPY_API_URL")
                ?? throw new InvalidOperationException("Variável de ambiente STEAMSPY_API_URL não definida.");

            Console.WriteLine($"[INFO] SteamSpyAcl inicializado com URL: {_urlGetAllGames}");
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Para conexões sem problemas SSL
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine($"[WARNING] Problema SSL detectado: {sslPolicyErrors}");
            
            // Log detalhado do certificado para debug
            if (certificate != null)
            {
                Console.WriteLine($"[DEBUG] Certificado: {certificate.Subject}");
                Console.WriteLine($"[DEBUG] Issuer: {certificate.Issuer}");
            }

            // Para desenvolvimento/teste, aceita alguns tipos de erro SSL
            // IMPORTANTE: Para produção, implemente validação mais rigorosa
            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch) ||
                sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
            {
                Console.WriteLine("[WARNING] Ignorando erro SSL - apenas para desenvolvimento");
                return true;
            }

            Console.WriteLine("[ERROR] Erro SSL crítico - conexão rejeitada");
            return false;
        }

        public async Task<(bool Success, JsonDocument? Data, string ErrorMessage)> GetAllGames()
        {
            const int maxRetries = 3;
            const int baseDelayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"[INFO] Tentativa {attempt}/{maxRetries} - Buscando dados do SteamSpy...");

                    HttpResponseMessage response = await _httpClient.GetAsync(_urlGetAllGames);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorMsg = $"API SteamSpy retornou erro - Status: {response.StatusCode}, Reason: {response.ReasonPhrase}";
                        Console.WriteLine($"[ERROR] {errorMsg}");
                        
                        // Se não é o último retry e o erro pode ser temporário, tenta novamente
                        if (attempt < maxRetries && IsRetryableStatusCode(response.StatusCode))
                        {
                            int delay = baseDelayMs * attempt; // Backoff exponencial
                            Console.WriteLine($"[INFO] Aguardando {delay}ms antes da próxima tentativa...");
                            await Task.Delay(delay);
                            continue;
                        }
                        
                        return (false, null, errorMsg);
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(responseBody))
                    {
                        string errorMsg = "API SteamSpy retornou resposta vazia";
                        Console.WriteLine($"[ERROR] {errorMsg}");
                        
                        if (attempt < maxRetries)
                        {
                            int delay = baseDelayMs * attempt;
                            Console.WriteLine($"[INFO] Aguardando {delay}ms antes da próxima tentativa...");
                            await Task.Delay(delay);
                            continue;
                        }
                        
                        return (false, null, errorMsg);
                    }

                    // Valida se o JSON é válido antes de retornar
                    JsonDocument doc;
                    try
                    {
                        doc = JsonDocument.Parse(responseBody);
                    }
                    catch (JsonException ex)
                    {
                        string errorMsg = $"Erro ao processar JSON da SteamSpy API: {ex.Message}";
                        Console.WriteLine($"[ERROR] {errorMsg}");
                        Console.WriteLine($"[DEBUG] Primeiros 200 caracteres da resposta: {responseBody.Substring(0, Math.Min(200, responseBody.Length))}");
                        return (false, null, errorMsg);
                    }

                    Console.WriteLine($"[SUCCESS] Dados obtidos com sucesso do SteamSpy ({responseBody.Length} caracteres)");
                    return (true, doc, string.Empty);
                }
                catch (HttpRequestException ex)
                {
                    string errorMsg = $"Erro de conexão com SteamSpy API: {ex.Message}";
                    Console.WriteLine($"[ERROR] Tentativa {attempt}/{maxRetries} - {errorMsg}");
                    
                    // Verifica se é erro SSL específico
                    if (ex.Message.Contains("SSL") || ex.Message.Contains("certificate"))
                    {
                        errorMsg += " - Problema de certificado SSL detectado";
                        Console.WriteLine("[INFO] Dica: Verifique se o certificado SSL do SteamSpy é válido");
                    }
                    
                    // Se não é o último retry, tenta novamente
                    if (attempt < maxRetries)
                    {
                        int delay = baseDelayMs * attempt * 2; // Backoff maior para erros de conexão
                        Console.WriteLine($"[INFO] Aguardando {delay}ms antes da próxima tentativa...");
                        await Task.Delay(delay);
                        continue;
                    }
                    
                    return (false, null, errorMsg);
                }
                catch (TaskCanceledException ex)
                {
                    string errorMsg = ex.InnerException is TimeoutException 
                        ? $"Timeout na SteamSpy API após {_httpClient.Timeout.TotalSeconds}s: {ex.Message}"
                        : $"Operação cancelada na SteamSpy API: {ex.Message}";
                    
                    Console.WriteLine($"[ERROR] Tentativa {attempt}/{maxRetries} - {errorMsg}");
                    
                    // Para timeout, tenta novamente com delay maior
                    if (attempt < maxRetries)
                    {
                        int delay = baseDelayMs * attempt * 3; // Backoff ainda maior para timeouts
                        Console.WriteLine($"[INFO] Aguardando {delay}ms antes da próxima tentativa...");
                        await Task.Delay(delay);
                        continue;
                    }
                    
                    return (false, null, errorMsg);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Erro inesperado na SteamSpy API: {ex.Message}";
                    Console.WriteLine($"[ERROR] {errorMsg}");
                    Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
                    throw new InvalidOperationException(errorMsg, ex);
                }
            }

            // Se chegou aqui, esgotou todas as tentativas
            return (false, null, $"Falha após {maxRetries} tentativas de conexão com SteamSpy API");
        }

        private static bool IsRetryableStatusCode(System.Net.HttpStatusCode statusCode)
        {
            return statusCode == System.Net.HttpStatusCode.InternalServerError ||
                   statusCode == System.Net.HttpStatusCode.BadGateway ||
                   statusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                   statusCode == System.Net.HttpStatusCode.GatewayTimeout ||
                   statusCode == System.Net.HttpStatusCode.TooManyRequests;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
                _disposed = true;
                Console.WriteLine("[INFO] SteamSpyAcl resources disposed");
            }
        }

        ~SteamSpyAcl()
        {
            Dispose(false);
        }
    }
}