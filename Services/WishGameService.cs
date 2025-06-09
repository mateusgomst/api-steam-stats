using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;

namespace APISTEAMSTATS.services;

public class WishGameService
{
    private readonly WishGameRepository _wishGameRepository;
    private readonly GameRepository _gameRepository;
    private readonly UserRepository _userRepository;

    public WishGameService(WishGameRepository wishGameRepository, GameRepository gameRepository, UserRepository userRepository)
    {
        _wishGameRepository = wishGameRepository;
        _gameRepository = gameRepository;
        _userRepository = userRepository;
    }

    public async Task<string> AddGame(int userid, Game game)
    {

        // ver se o jogo que vai ser adicionado existe na gameList
        Game findGame = await _gameRepository.FindGameByAppidPrimaryKey(game.AppId);
        if (findGame == null)
        {
            return "Jogo não existe!"; 
        }

        User user = await _userRepository.FindUserById(userid);

        WishGame wishGame = await _wishGameRepository.FindByUserIdAndAppId(userid, game.AppId);

        //tratar jogo repetido
        if (wishGame != null)
        {
            return "Esse jogo ja foi adcionado na sua WishList";
        }
        
        //tratar limite de jogos na wishlist
        if (user.CountListGames >= 10)
        {
            return "Você atingiu o limite de jogos na WishList!";
        }

        WishGame wishlist = new WishGame
        {
            UserId = userid,
            NameGame = game.NameGame,
            AppId = game.AppId,
            Discount = 0
        };

        await _wishGameRepository.Add(wishlist);
        await _userRepository.IncrementCountListGameByUserId(userid);
        return "";
    }
     
    public async Task RemoveGame(int userId, int appId)
    {
        var wishListItem = await _wishGameRepository.FindByUserIdAndAppId(userId, appId);
        if (wishListItem == null) throw new Exception("Jogo não encontrado na wishlist.");
        await _userRepository.DecrementCountListGameByUserId(userId);
        await _wishGameRepository.Remove(wishListItem);
    }

    public async Task<List<WishGame>> GetAllGamesByUserId(int userId)
    {
        return await _wishGameRepository.GetAllWishGamesByUserId(userId);
    }

}