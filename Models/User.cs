using System.ComponentModel.DataAnnotations;

namespace APISTEAMSTATS.models
{
    public class User
    {
        public int Id { get; set; }

        public string name { get; set; }

        [Required] public string login { get; set; }

        [Required] public string password { get; set; }

        public int countListGames { get; set; }
    }
}