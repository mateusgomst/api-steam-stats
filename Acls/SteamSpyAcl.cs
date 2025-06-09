using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace APISTEAMSTATS.services
{
    public class SteamSpyAcl
    {
        private readonly HttpClient _httpClient;
        private readonly string urlGetAllGames;

        public SteamSpyAcl()
        {
            _httpClient = new HttpClient();

            urlGetAllGames = Environment.GetEnvironmentVariable("STEAMSPY_API_URL")
                ?? throw new InvalidOperationException("Variável de ambiente STEAMSPY_API_URL não definida.");
        }

        public async Task<(bool Success, JsonDocument? Data, string ErrorMessage)> GetAllGames()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(urlGetAllGames);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg =
                        $"API SteamSpy retornou erro - Status: {response.StatusCode}, Reason: {response.ReasonPhrase}";
                    return (false, null, errorMsg);
                }

                string responseBody = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    return (false, null, "API SteamSpy retornou resposta vazia");
                }

                JsonDocument doc = JsonDocument.Parse(responseBody);
                return (true, doc, string.Empty);
            }
            catch (HttpRequestException ex)
            {
                string errorMsg = $"Erro de conexão com SteamSpy API: {ex.Message}";
                return (false, null, errorMsg);
            }
            catch (TaskCanceledException ex)
            {
                string errorMsg = $"Timeout na SteamSpy API: {ex.Message}";
                return (false, null, errorMsg);
            }
            catch (JsonException ex)
            {
                string errorMsg = $"Erro ao processar JSON da SteamSpy API: {ex.Message}";
                return (false, null, errorMsg);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro inesperado na SteamSpy API: {ex.Message}", ex);
            }
        }
    }
}
