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

        public async Task<User?> Register(User user)
        {
            var userFound = await _userRepository.FindUser(user);
            if (userFound != null)
            {
                return null;
            }
            user.CountListGames = 0;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);


            await _userRepository.AddUser(user);
            return user;
        }
        
        public async Task<User?> Login(User user)
        {
            var userFound = await _userRepository.FindUser(user);
            if (userFound == null)
            {
                return null;
            }
            user.Id=userFound.Id;
            
            bool success = BCrypt.Net.BCrypt.Verify(user.Password, userFound.Password);
            if (!success)
            {
                return null;
            }
            return user;
        }
        
        
        
    }
}