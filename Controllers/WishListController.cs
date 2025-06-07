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

    public class WishListController : ControllerBase
    {
        private readonly WishListService _wishListService;

        public WishListController(WishListService wishListService)
        {
            _wishListService = wishListService;
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> AddWish(GameList game)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            int id = int.Parse(userId);
            string addGame =await _wishListService.AddGame(id, game);

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
            
            await _wishListService.RemoveGame(id, appId);

            return Ok();
        }

        
            
        
    }
    
    
    
}