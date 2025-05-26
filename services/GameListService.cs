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
                using JsonDocument allGamesDocument = await _steamSpyAcl.GetAllGames();
                JsonElement allGames = allGamesDocument.RootElement;

                var newGames = new List<GameList>();
                var gamesToUpdate = new List<GameList>();

                // Busca jogos existentes e evita duplicatas por appId
                var existingGames = await _gameListRepository.GetAllGames();
                var existingGameDict = existingGames
                    .GroupBy(g => g.appId)
                    .Select(g => g.First())
                    .ToDictionary(g => g.appId);

                foreach (JsonProperty jogo in allGames.EnumerateObject())
                {
                    var appid = jogo.Value.GetProperty("appid").GetInt32();
                    var name = jogo.Value.GetProperty("name").GetString();
                    var positive = jogo.Value.GetProperty("positive").GetInt32();
                    var discount = jogo.Value.GetProperty("discount").GetString();
                    int discountInt = int.Parse(discount);


                    if (existingGameDict.TryGetValue(appid, out var existingGame))
                    {
                        // Atualiza o desconto se mudou
                        if (existingGame.discount != discountInt)
                        {
                            existingGame.discount = discountInt;
                            gamesToUpdate.Add(existingGame);
                        }
                    }
                    else
                    {
                        // Adiciona novo jogo à lista
                        newGames.Add(new GameList
                        {
                            appId = appid,
                            nameGame = name,
                            positive = positive,
                            discount = discountInt
                        });
                    }
                }

                // Salva novos jogos no banco
                if (newGames.Any())
                    await _gameListRepository.AddListInGames(newGames);

                // Atualiza descontos de jogos existentes
                if (gamesToUpdate.Any())
                    await _gameListRepository.UpdateGamesDiscount(gamesToUpdate);
            }
            catch (HttpRequestException e)
            {
                throw new Exception("Erro ao obter jogos da API SteamSpy: " + e.Message);
            }
        }


    }
}