using APISTEAMSTATS.data;
using APISTEAMSTATS.models;
using Microsoft.EntityFrameworkCore; 

namespace APISTEAMSTATS.repository
{
    public class UserRepository
    {

        private readonly AppDbContext _appDbContext;
        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<User?> FindUser(User user)
        {
            var userFound = await _appDbContext.users.FirstOrDefaultAsync(u => u.Login == user.Login);
            return userFound;
        }

        public async Task<User?> FindUserById(int userId)
        {
            var userFound = await _appDbContext.users.FirstOrDefaultAsync(u => u.Id == userId);
            return userFound;
        }

        public async Task<User?> AddUser(User user)
        {
            await _appDbContext.users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();
            return user;
        }

        public async Task IncrementCountListGameByUserId(int userId)
        {
            var user = await _appDbContext.users.FirstOrDefaultAsync(u => u.Id == userId);
            user.CountListGames += 1;
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DecrementCountListGameByUserId(int userId)
        {
            var user = await _appDbContext.users.FirstOrDefaultAsync(u => u.Id == userId);
            user.CountListGames -= 1;
            await _appDbContext.SaveChangesAsync();
        }

    }
}