using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;

namespace APISTEAMSTATS.services;

public class DailyTaskService
{
    private readonly GameListService _gameListService;
    private readonly WishListRepository _wishListRepository;
    private readonly GameListRepository _gameListRepository;
    
    public DailyTaskService(GameListService gameListService, WishListRepository wishListRepository,GameListRepository gameListRepository)
    {
        _gameListService = gameListService;
        _wishListRepository = wishListRepository;
        _gameListRepository = gameListRepository;
    }

    public async Task<bool> Task()
    {
        await _gameListService.UploadAllGames();
        
        List<WishList> wishList = await _wishListRepository.GetAllWishList();

        foreach (WishList wish in wishList)
        {
            int appId = wish.idGame;
            int discount = wish.discount;

            GameList game = await _gameListRepository.FindGameByAppidPrimaryKey(appId);
            
            if (discount < game.discount)
            {
                Console.WriteLine("Notificado");
                return true;
            }
            
        }

        return false;
    }
}