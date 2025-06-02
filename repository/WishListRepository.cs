using APISTEAMSTATS.data;
using APISTEAMSTATS.models;
using Microsoft.EntityFrameworkCore;

namespace APISTEAMSTATS.repository;

public class WishListRepository
{
    private readonly AppDbContext _appDbContext;

    public WishListRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task add(WishList wishlist)
    {
        await _appDbContext.wishlists.AddAsync(wishlist);
        await _appDbContext.SaveChangesAsync();
    }
    
    public async Task<WishList?> Add(int appid, int userid)
    {
        return await _appDbContext.wishlists
            .FirstOrDefaultAsync(w => w.idGame == appid && w.userId == userid);
    }


    public async Task<WishList?> FindByUserIdAndAppId(int userId, int appId)
    {
        return await _appDbContext.wishlists
            .FirstOrDefaultAsync(w => w.userId == userId && w.idGame == appId);
    }

    public async Task Remove(WishList wishlist)
    {
        _appDbContext.wishlists.Remove(wishlist);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<List<WishList>> GetAllWishList()
    {
        return await _appDbContext.wishlists.ToListAsync();
    }

    
}