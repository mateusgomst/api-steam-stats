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

    public async Task<string> AddGame(int userid, GameList game)
    {

        // ver se o jogo que vai ser adicionado existe na gameList
        GameList findGame = await _gameListRepository.FindGameByAppidPrimaryKey(game.appId);
        if (findGame == null)
        {
            return "Jogo não existe!"; 
        }

        User user = await _userRepository.FindUserById(userid);

        WishGame wishGame = await _wishListRepository.FindByUserIdAndAppId(userid, game.appId);

        //tratar jogo repetido
        if (wishGame != null)
        {
            return "Esse jogo ja foi adcionado na sua WishList";
        }
        
        //tratar limite de jogos na wishlist
        if (user.countListGames >= 10)
        {
            return "Você atingiu o limite de jogos na WishList!";
        }

        WishGame wishlist = new WishGame
        {
            UserId = userid,
            NameGame = game.nameGame,
            GameId = game.appId,
            Discount = 0
        };

        await _wishListRepository.Add(wishlist);
        await _userRepository.IncrementCountListGameByUserId(userid);
        return "";
    }
     
    public async Task RemoveGame(int userId, int appId)
    {
        var wishListItem = await _wishListRepository.FindByUserIdAndAppId(userId, appId);
        if (wishListItem == null) throw new Exception("Jogo não encontrado na wishlist.");
        await _userRepository.DecrementCountListGameByUserId(userId);
        await _wishListRepository.Remove(wishListItem);
    }

}