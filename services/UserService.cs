using APISTEAMSTATS.data;
using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;

namespace APISTEAMSTATS.services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> Register(User user)
        {
            
            var userFound = await _userRepository.FindUser(user);
            if (userFound != null)
            {
                return userFound;
            }
            
            
            return userFound;//so para sair o erro, tem q tirar dps
        }
        

    }
}