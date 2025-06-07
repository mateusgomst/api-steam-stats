using APISTEAMSTATS.data;
using APISTEAMSTATS.models;
using Microsoft.EntityFrameworkCore;

namespace APISTEAMSTATS.repository
{
    public class GameListRepository
    {
        private readonly AppDbContext _appDbContext;

        public GameListRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<GameList>> GetAllGames()
        {
            List<GameList> gameList = await _appDbContext.games.ToListAsync();
            return gameList;
        }

        public async Task UpdateGamesDiscount(List<GameList> gamesToUpdate)
        {
            _appDbContext.games.UpdateRange(gamesToUpdate);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<GameList?> FindGameByAppidPrimaryKey(int appid)
        {
            var existingGame = await _appDbContext.games
                .FirstOrDefaultAsync(g => g.appId == appid);
            return existingGame;
        }

        public async Task AddListInGames(List<GameList> gameList)
        {
            _appDbContext.games.AddRange(gameList);
            await _appDbContext.SaveChangesAsync();
        }
    }
}