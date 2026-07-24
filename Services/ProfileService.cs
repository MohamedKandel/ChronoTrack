using Microsoft.EntityFrameworkCore;

public class ProfileService
{
    private readonly AppDBContext _context;

    public ProfileService(AppDBContext context)
    {
        _context = context;
    }

    public async Task<ProfileDTO> GetProfile(string baseUrl, string accountID)
    {
        var account = await _context.Accounts
        .FirstOrDefaultAsync(a => a.AccountID == accountID);

        if (account == null)
        {
            return new ProfileDTO
            {
                status = ResponseStatus.NotFound
            };
        }

        if (!account.IsVerified)
        {
            return new ProfileDTO
            {
                status = ResponseStatus.NotVerified
            };
        }

        return new ProfileDTO
        {
            status = ResponseStatus.Success,
            Username = account.Username,
            ProfilePic = $"{baseUrl}/Profile/api/images/{account.AccountID}"
        };
    }

    public async Task<(byte[]? Image, string? ContentType)> GetImage(string accountID)
    {
        var account = await _context.Accounts
                .Where(a => a.AccountID == accountID)
                .Select(a => new
                {
                    a.Image,
                    a.ContentType
                })
                .FirstOrDefaultAsync();
        
        if(account == null)
        {
            return (null,null);
        }

        return (account.Image, account.ContentType);
    }
}