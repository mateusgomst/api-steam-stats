using APISTEAMSTATS.models;
using APISTEAMSTATS.services;
using Microsoft.AspNetCore.Mvc;

namespace APISTEAMSTATS.controllers
{
    [ApiController]
    [Route("games")]
    public class GamesController : ControllerBase
    {
        private readonly GameListService _gameListService;

        public GamesController(GameListService gameListService)
        {
            _gameListService = gameListService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadGames()
        {
            await _gameListService.GetAllGames();
            return Ok("Jogos carregados!");
        }

        [HttpGet]
        public async Task<List<GameList>> GameList()
        {
            List<GameList> games = await _gameListService.GetAllGames();
            return games;
        }

    }
}