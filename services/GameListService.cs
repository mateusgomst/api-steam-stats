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
        private readonly SteamSpyAcl _steamSpyAcl;
        private string urlGetAllGames = "https://steamspy.com/api.php?request=all";

        public GameListService(AppDbContext appDbContext, SteamSpyAcl steamSpyAcl)
        {
            _appDbContext = appDbContext;
            _httpClient = new HttpClient();
            _steamSpyAcl = steamSpyAcl;
        }

        public async Task GetAllGames()
        {
            try
            {
                //acl flurl
                JsonElement allGames = await _steamSpyAcl.GetAllGames();
                
                //repository
                await _appDbContext.Database.ExecuteSqlRawAsync("DELETE FROM games"); // alterar a logica para verificar 
                // se tme um jogo novo, caso tenha adcione ele

                await _appDbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE games RESTART IDENTITY");

                var gameList = new List<GameList>();

                //service
                foreach (JsonProperty jogo in allGames.EnumerateObject())
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

                //repository
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
