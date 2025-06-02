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
        private readonly DailyTaskService _dailyTaskService;

        public GamesController(GameListService gameListService, DailyTaskService dailyTaskService)
        {
            _gameListService = gameListService;
            _dailyTaskService = dailyTaskService;
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

        [HttpGet("teste")]
        public async Task<IActionResult> testeEmail()
        {
            await _dailyTaskService.ExecuteDailyTask();
            return Ok();
        }

    }
}