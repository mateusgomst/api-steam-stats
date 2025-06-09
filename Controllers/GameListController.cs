using APISTEAMSTATS.models;
using APISTEAMSTATS.services;
using Microsoft.AspNetCore.Mvc;

namespace APISTEAMSTATS.controllers
{
    [ApiController]
    [Route("games")]
    public class GamesController : ControllerBase
    {
        private readonly GameService _gameService;
        private readonly DailyTaskService _dailyTaskService;

        public GamesController(GameService gameService, DailyTaskService dailyTaskService)
        {
            _gameService = gameService;
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
        public async Task<ActionResult<List<Game>>> GameList()
        {
            var games = await _gameService.GetAllGames();
            return Ok(games);
        }
    }
}