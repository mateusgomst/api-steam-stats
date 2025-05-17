using APISTEAMSTATS.services;
using Microsoft.AspNetCore.Mvc;

namespace APISTEAMSTATS.controllers
{
    [ApiController]
    [Route("api")]
    public class GamesController : ControllerBase
    {
        private readonly GameListService _gameListService;

        public GamesController(GameListService gameListService)
        {
            _gameListService = gameListService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadGames()
        {
            await _gameListService.GetAllGames();
            return Ok("Jogos carregados!");
        }
    }
}