using APISTEAMSTATS.data;
using APISTEAMSTATS.models;

namespace APISTEAMSTATS.repository
{
    public class GameListRepository
    {

        private readonly AppDbContext _appDbContext;
        public GameListRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<GameList?> FindGameByAppid(int appid)
        {
            var existingGame = await _appDbContext.games.FindAsync(appid);
            return existingGame;
        }

        public async Task AddListInGames(List<GameList> gameList)
        {
            _appDbContext.games.AddRange(gameList);
            await _appDbContext.SaveChangesAsync();
        }




    }
}