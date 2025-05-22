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
            var userFound = await _appDbContext.users.FirstOrDefaultAsync(u => u.login == user.login);
            return userFound;
        }

        public async Task<User?> AddUser(User user)
        {
            await _appDbContext.users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();
            return user;
        }

    }
}