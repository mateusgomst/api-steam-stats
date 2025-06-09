using APISTEAMSTATS.data;
using APISTEAMSTATS.models;
using Microsoft.EntityFrameworkCore;

namespace APISTEAMSTATS.repository
{
    public class GameRepository
    {
        private readonly AppDbContext _appDbContext;

        public GameRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<Game>> GetAllGames()
        {
            List<Game> gameList = await _appDbContext.games.ToListAsync();
            return gameList;
        }

        public async Task UpdateGamesDiscount(List<Game> gamesToUpdate)
        {
            _appDbContext.games.UpdateRange(gamesToUpdate);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<Game?> FindGameByAppidPrimaryKey(int appid)
        {
            var existingGame = await _appDbContext.games
                .FirstOrDefaultAsync(g => g.AppId == appid);
            return existingGame;
        }

        public async Task AddListInGames(List<Game> gameList)
        {
            _appDbContext.games.AddRange(gameList);
            await _appDbContext.SaveChangesAsync();
        }
    }
}