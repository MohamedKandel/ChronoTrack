using Microsoft.EntityFrameworkCore;

public class SessionService
{
    private readonly AppDBContext _context;

    public SessionService(AppDBContext context)
    {
        _context = context;
    }

    private async Task<string> GenerateUniqueSessionId()
    {
        string sessionID;
        do
        {
            sessionID = Utils.GenerateId();
        } while (await _context.Sessions.AnyAsync(x => x.SessionId == sessionID));

        return sessionID;
    }

    public async Task<ResponseStatus> AddSession(AddSessionDTO dto, string accountID)
    {
        var sessionID = await GenerateUniqueSessionId();

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountID == accountID);

        if (account == null)
        {
            return ResponseStatus.NotFound;
        }

        var sessionObj = new Sessions
        {
            SessionId = sessionID,
            SessionDate = dto.SessionDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            AccountId = accountID,
            Account = account
        };

        _context.Sessions.Add(sessionObj);

        await _context.SaveChangesAsync();

        return ResponseStatus.Success;
    }

    public async Task<ResponseStatus> EndSession(string sessionID)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionID);

        if (session == null)
        {
            return ResponseStatus.NotFound;
        }

        if (session.EndTime != null)
        {
            return ResponseStatus.Invalid;
        }

        if(DateOnly.FromDateTime(DateTime.Now) > session.SessionDate)
        {
            return ResponseStatus.OldSession;
        }

        session.EndTime = TimeOnly.FromDateTime(DateTime.Now);

        await _context.SaveChangesAsync();

        return ResponseStatus.Success;
    }

    public async Task<PagedResult<SessionDataDTO>> GetFilteredSessions(PaginationParams paginationParams, string accountID, FilteredSessions? filteration)
    {
        var query = _context.SessionsViews
            .AsNoTracking()
            .Where(s => s.AccountID == accountID);

        if (filteration != null)
        {
            if (filteration.SessionDate.HasValue)
            {
                query = query.Where(s => s.SessionDate == filteration.SessionDate);
            }

            if (filteration.Month != null)
            {
                query = query.Where(s => s.SessionDate.Month == filteration.Month);
            }

            if (filteration.Year != null)
            {
                query = query.Where(s => s.SessionDate.Year == filteration.Year);
            }
        }

        query = query.OrderBy(s => s.SessionDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(s => new SessionDataDTO
            {
                SessionId = s.SessionId,
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Duration = s.Duration,
                RoundedHours = s.RoundedHours,
            })
            .ToListAsync();

        return new PagedResult<SessionDataDTO>
        {
            Items = items,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SessionStatistics> GetStatistics(string accountID)
    {
        var now = DateTime.Now;

        var currentMonthHours = await _context.SessionsViews
            .Where(s =>
                s.AccountID == accountID &&
                s.SessionDate.Year == now.Year &&
                s.SessionDate.Month == now.Month)
            .SumAsync(s => s.RoundedHours ?? 0);

        var previousMonth = DateTime.Now.AddMonths(-1);

        var previousMonthHours = await _context.SessionsViews
            .Where(s =>
                s.AccountID == accountID &&
                s.SessionDate.Year == previousMonth.Year &&
                s.SessionDate.Month == previousMonth.Month)
            .SumAsync(s => s.RoundedHours ?? 0);

        var percent = previousMonthHours == 0 ? 0 : 
            ((currentMonthHours - previousMonthHours) / previousMonthHours) * 100;

        var activeSessionID = await _context.SessionsViews
            .Where(s =>
                s.AccountID == accountID &&
                s.EndTime == null)
            .Select(s => s.SessionId)
            .FirstOrDefaultAsync() ?? string.Empty;

        var lastSession = await _context.SessionsViews
            .Where(s =>
                s.AccountID == accountID &&
                s.EndTime != null)
            .OrderByDescending(s => s.SessionDate)
            .FirstOrDefaultAsync()
            ?? new SessionsView();

        var statisticsObject = new SessionStatistics
        {
            HoursThisMonth = currentMonthHours,
            HoursLastMonth = previousMonthHours,
            Percent = $"{percent}%",
            ActiveSessionID = activeSessionID,
            LastSession = lastSession
        };
        
        return statisticsObject;
    }
}