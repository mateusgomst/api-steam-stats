using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;

namespace APISTEAMSTATS.services
{
    public class GameService
    {
        private readonly SteamSpyAcl _steamSpyAcl;
        private readonly GameRepository _gameRepository;

        public GameService(SteamSpyAcl steamSpyAcl, GameRepository gameRepository)
        {
            _steamSpyAcl = steamSpyAcl;
            _gameRepository = gameRepository;
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
                var existingGames = await _gameRepository.GetAllGames();
                var existingGameDict = existingGames.ToDictionary(g => g.AppId);

                var newGames = new List<Game>();
                var gamesToUpdate = new List<Game>();

                foreach (var jogo in allGames.EnumerateObject())
                {
                    var gameElement = jogo.Value;

                    var appid = gameElement.GetProperty("appid").GetInt32();
                    var name = gameElement.GetProperty("name").GetString();
                    var positive = gameElement.GetProperty("positive").GetInt32();
                    var discount = int.Parse(gameElement.GetProperty("discount").GetString());

                    if (existingGameDict.TryGetValue(appid, out var existingGame))
                    {
                        if (existingGame.Discount != discount)
                        {
                            existingGame.Discount = discount;
                            gamesToUpdate.Add(existingGame);
                        }
                    }
                    else
                    {
                        newGames.Add(new Game
                        {
                            AppId = appid,
                            NameGame = name,
                            Positive = positive,
                            Discount = discount
                        });
                    }
                }

                if (newGames.Any())
                    await _gameRepository.AddListInGames(newGames);

                if (gamesToUpdate.Any())
                    await _gameRepository.UpdateGamesDiscount(gamesToUpdate);

                Console.WriteLine("[SUCCESS] Sincronização concluída");
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<List<Game>> GetAllGames()
        {
            return await _gameRepository.GetAllGames();
        }
    }
}