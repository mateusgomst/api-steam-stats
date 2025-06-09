using System.Security.Claims;
using APISTEAMSTATS.models;
using APISTEAMSTATS.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace APISTEAMSTATS.controllers
{
    [ApiController]
    [Route("wishlist")]

    public class WishGameController : ControllerBase
    {
        private readonly WishGameService _wishGameService;

        public WishGameController(WishGameService wishGameService)
        {
            _wishGameService = wishGameService;
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> AddWish(Game game)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            int id = int.Parse(userId);
            string addGame =await _wishGameService.AddGame(id, game);

            if (addGame != "") return NotFound(addGame);

            return Ok();
        }
        
        [Authorize]
        [HttpDelete("{appId}")]
        public async Task<IActionResult> RemoveWishList(int appId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            int id = int.Parse(userId);
            
            await _wishGameService.RemoveGame(id, appId);

            return Ok();
        }
        
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AllWishGamesByUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            int id = int.Parse(userId);
            
            List<WishGame> allGames= await _wishGameService.GetAllGamesByUserId(id);

            return Ok(allGames);
        }

        
            
        
    }
    
    
    
}