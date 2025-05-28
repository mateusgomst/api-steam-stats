namespace APISTEAMSTATS.models;

public class WishList
{
    public int Id { get; set; }
    
    public int userId { get; set; }
    
    public string nameGame { get; set; }
    
    public int idGame { get; set; }
    
    public int discount { get; set; }
}