using System.Text.Json;
using APISTEAMSTATS.data;
using APISTEAMSTATS.models;
using Microsoft.EntityFrameworkCore;

namespace APISTEAMSTATS.services
{
    public class GameListService
    {
        private readonly AppDbContext _appDbContext;
        private readonly HttpClient _httpClient;
        private string urlGetAllGames = "https://steamspy.com/api.php?request=all";

        public GameListService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _httpClient = new HttpClient();
        }

        public async Task GetAllGames()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(urlGetAllGames);
                response.EnsureSuccessStatusCode();
                
                string responseBody = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;
                
                await _appDbContext.Database.ExecuteSqlRawAsync("DELETE FROM games");
                await _appDbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE games RESTART IDENTITY");

                var gameList = new List<GameList>();

                foreach (JsonProperty jogo in root.EnumerateObject())
                {
                    var positive = jogo.Value.GetProperty("positive").GetInt32();
                    var appid = jogo.Value.GetProperty("appid").GetInt32();
                    var name = jogo.Value.GetProperty("name").GetString();

                    gameList.Add(new GameList
                    {
                        positive = positive,
                        appId = appid,
                        nameGame = name
                    });
                }

                _appDbContext.games.AddRange(gameList);
                await _appDbContext.SaveChangesAsync();

            }
            catch (HttpRequestException e)
            {
                throw new Exception("Erro ao obter jogos da API SteamSpy: " + e.Message);
            }
        }
    }
}
