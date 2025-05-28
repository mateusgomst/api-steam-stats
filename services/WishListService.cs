using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;

namespace APISTEAMSTATS.services;

public class WishListService
{
    private readonly WishListRepository _wishListRepository;

    public WishListService(WishListRepository wishListRepository)
    {
        _wishListRepository = wishListRepository;
    }

     public async Task AddGame(int userid, GameList game)
    {   //falta tratar para nao deixar adcionar um jogo aleatorio, tratar o appid do jogo para ver se existe no banco
        //tratar jogo repetido
        //incrementar um na coluna countListGames para um jogo adcionado na wishlist
        //adcionar um limite de 10 jogos por userid
        WishList wishlist = new WishList{
            userId = userid,
            nameGame = game.nameGame,
            idGame = game.appId,
            discount = game.discount
        };
        
        await _wishListRepository.add(wishlist);
        
        //userId, nameGame, appId, discount
    }
     
    public async Task RemoveGame(int userId, int appId)
    {
        var wishListItem = await _wishListRepository.FindByUserIdAndAppId(userId, appId);
        if (wishListItem == null)
            throw new Exception("Jogo não encontrado na wishlist.");

        await _wishListRepository.Remove(wishListItem);
    }

}