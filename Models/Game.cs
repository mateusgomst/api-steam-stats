namespace APISTEAMSTATS.models
{
    public class Game
    {
        public int Id { get; set; }
        public string NameGame { get; set; }
        public int AppId { get; set; }
        public int Positive { get; set; }

        public int Discount { get; set; }
    }
}