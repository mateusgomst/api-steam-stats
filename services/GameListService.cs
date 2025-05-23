using System.Text.Json;
using APISTEAMSTATS.data;
using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;
using Microsoft.EntityFrameworkCore;

namespace APISTEAMSTATS.services
{
    public class GameListService
    {
        private readonly SteamSpyAcl _steamSpyAcl;
        private readonly GameListRepository _gameListRepository;

        public GameListService(
            SteamSpyAcl steamSpyAcl,
            GameListRepository gameListRepository)
        {
            _steamSpyAcl = steamSpyAcl;
            _gameListRepository = gameListRepository;
        }

        public async Task GetAllGames()
        {
            try
            {
                JsonElement allGames = await _steamSpyAcl.GetAllGames();
                var gameList = new List<GameList>();

                foreach (JsonProperty jogo in allGames.EnumerateObject())
                {
                    var positive = jogo.Value.GetProperty("positive").GetInt32();
                    var appid = jogo.Value.GetProperty("appid").GetInt32();
                    var name = jogo.Value.GetProperty("name").GetString();

                    var existingGame = await _gameListRepository.FindGameByAppid(appid);

                    if (existingGame == null)
                    {
                        gameList.Add(new GameList
                        {
                            positive = positive,
                            appId = appid,
                            nameGame = name
                        });
                    }
                }
                await _gameListRepository.AddListInGames(gameList);
            }
            catch (HttpRequestException e)
            {
                throw new Exception("Erro ao obter jogos da API SteamSpy: " + e.Message);
            }
        }
    }
}
