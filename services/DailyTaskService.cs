using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;
using APISTEAMSTATS.services;

public class DailyTaskService
{
    private readonly GameListService _gameListService;
    private readonly WishListRepository _wishListRepository;
    private readonly GameListRepository _gameListRepository;
    private readonly EmailAcl _emailAcl;
    private readonly UserRepository _userRepository;

    public DailyTaskService(GameListService gameListService, WishListRepository wishListRepository,
                            GameListRepository gameListRepository, EmailAcl emailAcl, UserRepository userRepository)
    {
        _gameListService = gameListService;
        _wishListRepository = wishListRepository;
        _gameListRepository = gameListRepository;
        _emailAcl = emailAcl;
        _userRepository = userRepository;
    }

    public async Task ExecuteDailyTask()
    {
        try
        {
            await _gameListService.UploadAllGames();
            List<WishList> wishList = await _wishListRepository.GetAllWishList();

            foreach (WishList wish in wishList)
            {
                int appId = wish.idGame;
                int wishDiscount = wish.discount;

                GameList game = await _gameListRepository.FindGameByAppidPrimaryKey(appId);

                if (game.discount > wishDiscount)
                {
                    User user = await _userRepository.FindUserById(wish.userId);
                     
                    await _emailAcl.SendPromotionEmail(
                        toEmail: user.login, 
                        gameName: game.nameGame,
                        discount: game.discount,
                        game.appId
                    );
                            
                    Console.WriteLine("E-mail enviado com sucesso!");
                       
                }
                    
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na execução da tarefa diária: {ex.Message}");
        }
    }
}