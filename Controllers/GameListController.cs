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

        /*[HttpPost]
        public async Task<IActionResult> UploadGames()
        {
            bool success = await _gameListService.UploadAllGames();

            if (success)
                return Ok("Jogos carregados com sucesso!");
            else
                return StatusCode(500, "Falha ao carregar os jogos.");
        }
*/
        [HttpGet]
        public async Task<ActionResult<List<GameList>>> GameList()
        {
            var games = await _gameListService.GetAllGames();
            return Ok(games);
        }
    }
}