using APISTEAMSTATS.data;

namespace APISTEAMSTATS.services
{
    public class UserService
    {
        private readonly AppDbContext _appDbContext;

        public UserService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        

    }
}