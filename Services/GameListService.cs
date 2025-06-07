using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;

namespace APISTEAMSTATS.services
{
    public class GameListService
    {
        private readonly SteamSpyAcl _steamSpyAcl;
        private readonly GameListRepository _gameListRepository;

        public GameListService(SteamSpyAcl steamSpyAcl, GameListRepository gameListRepository)
        {
            _steamSpyAcl = steamSpyAcl;
            _gameListRepository = gameListRepository;
        }

        public async Task<(bool Success, string ErrorMessage)> UploadAllGames()
        {
            try
            {
                Console.WriteLine("[INFO] Iniciando sincronização...");
                
                var (success, allGamesDocument, errorMessage) = await _steamSpyAcl.GetAllGames();
                if (!success || allGamesDocument == null)
                {
                    Console.WriteLine("[ERROR] Falha na API");
                    return (false, errorMessage);
                }

                var allGames = allGamesDocument.RootElement;
                var existingGames = await _gameListRepository.GetAllGames();
                var existingGameDict = existingGames.ToDictionary(g => g.appId);

                var newGames = new List<GameList>();
                var gamesToUpdate = new List<GameList>();

                foreach (var jogo in allGames.EnumerateObject())
                {
                    var gameElement = jogo.Value;

                    var appid = gameElement.GetProperty("appid").GetInt32();
                    var name = gameElement.GetProperty("name").GetString();
                    var positive = gameElement.GetProperty("positive").GetInt32();
                    var discount = int.Parse(gameElement.GetProperty("discount").GetString());

                    if (existingGameDict.TryGetValue(appid, out var existingGame))
                    {
                        if (existingGame.discount != discount)
                        {
                            existingGame.discount = discount;
                            gamesToUpdate.Add(existingGame);
                        }
                    }
                    else
                    {
                        newGames.Add(new GameList
                        {
                            appId = appid,
                            nameGame = name,
                            positive = positive,
                            discount = discount
                        });
                    }
                }

                if (newGames.Any())
                    await _gameListRepository.AddListInGames(newGames);

                if (gamesToUpdate.Any())
                    await _gameListRepository.UpdateGamesDiscount(gamesToUpdate);

                Console.WriteLine("[SUCCESS] Sincronização concluída");
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<List<GameList>> GetAllGames()
        {
            return await _gameListRepository.GetAllGames();
        }
    }
}