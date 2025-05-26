using System.Text.Json;
using APISTEAMSTATS.data;

namespace APISTEAMSTATS.services;

    public class SteamSpyAcl
    {
        
        private readonly HttpClient _httpClient;
        private string urlGetAllGames = "https://steamspy.com/api.php?request=all";

        public SteamSpyAcl()
        {
            _httpClient = new HttpClient();
        }

        public async Task<JsonDocument> GetAllGames()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(urlGetAllGames);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument doc = JsonDocument.Parse(responseBody);

                return doc;
            }
            catch (HttpRequestException e)
            {
                throw new Exception("Erro ao obter jogos da API SteamSpy: " + e.Message);
            }
        }
    }