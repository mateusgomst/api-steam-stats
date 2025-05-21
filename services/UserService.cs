using APISTEAMSTATS.data;
using APISTEAMSTATS.models;

namespace APISTEAMSTATS.services
{
    public class UserService
    {
        private readonly AppDbContext _appDbContext;

        public UserService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task Register(User user)
        {
            
            var userFound = await _appDbContext.users.FindAsync(user.login);
            if (userFound != null)
            {
                throw new Exception("Usuario ja cadastrado");
            }
            

        }
        

    }
}