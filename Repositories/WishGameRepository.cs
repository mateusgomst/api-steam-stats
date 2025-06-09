using APISTEAMSTATS.data;
using APISTEAMSTATS.models;
using Microsoft.EntityFrameworkCore;

namespace APISTEAMSTATS.repository;

public class WishGameRepository
{
    private readonly AppDbContext _appDbContext;

    public WishGameRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task Add(WishGame wishlist)
    {
        await _appDbContext.wishlists.AddAsync(wishlist);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<WishGame?> FindByUserIdAndAppId(int userId, int appId)
    {
        return await _appDbContext.wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.AppId == appId);
    }

    public async Task Remove(WishGame wishlist)
    {
        _appDbContext.wishlists.Remove(wishlist);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<List<WishGame>> GetAllWishList()
    {
        return await _appDbContext.wishlists.ToListAsync();
    }

    public async Task<List<WishGame>> GetAllWishGamesByUserId(int userId)
    {
        return await _appDbContext.wishlists.Where(w => w.UserId == userId).ToListAsync();
    }

    
}