using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;

namespace APISTEAMSTATS.services;

public class WishListService
{
    private readonly WishListRepository _wishListRepository;
    private readonly GameListRepository _gameListRepository;
    private readonly UserRepository _userRepository;

    public WishListService(WishListRepository wishListRepository, GameListRepository gameListRepository, UserRepository userRepository)
    {
        _wishListRepository = wishListRepository;
        _gameListRepository = gameListRepository;
        _userRepository = userRepository;
    }

    public async Task<bool?> AddGame(int userid, GameList game)
    {

        // ver se o jogo que vai ser adicionado existe na gameList
        GameList findGame = await _gameListRepository.FindGameByAppidPrimaryKey(game.appId);
        if (findGame == null)
        {
            return null; 
        }

        User user = await _userRepository.FindUserById(userid);

        WishList wishList = await _wishListRepository.FindByUserIdAndAppId(userid, game.appId);

        //tratar jogo repetido
        if (wishList != null)
        {
            return null;
        }
        
        //tratar limite de jogos na wishlist
        if (user.countListGames >= 10)
        {
            return null;
        }

        WishList wishlist = new WishList
        {
            userId = userid,
            nameGame = game.nameGame,
            idGame = game.appId,
            discount = 0
        };

        await _wishListRepository.Add(wishlist);
        await _userRepository.IncrementCountListGameByUserId(userid);
        return true;
        //userId, nameGame, appId, discount
    }
     
    public async Task RemoveGame(int userId, int appId)
    {
        var wishListItem = await _wishListRepository.FindByUserIdAndAppId(userId, appId);
        if (wishListItem == null) throw new Exception("Jogo não encontrado na wishlist.");
        await _userRepository.DecrementCountListGameByUserId(userId);
        await _wishListRepository.Remove(wishListItem);
    }

}