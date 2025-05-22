using APISTEAMSTATS.data;
using APISTEAMSTATS.models;

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
            var userFound = await _appDbContext.users.FindAsync(user.login);
            return userFound;
        }

    }
}