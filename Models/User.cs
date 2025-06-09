using System.ComponentModel.DataAnnotations;

namespace APISTEAMSTATS.models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Required] public string Login { get; set; }

        [Required] public string Password { get; set; }

        public int CountListGames { get; set; }
    }
}