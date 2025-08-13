using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Match entity
/// </summary>
public class MatchRepository : IMatchRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MatchRepository> _logger;

    public MatchRepository(ApplicationDbContext context, ILogger<MatchRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Match?> GetByEspnIdAsync(string espnId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Matches
                .FirstOrDefaultAsync(m => m.EspnId == espnId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get match by ESPN ID: {EspnId}", espnId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<Match>> GetBySeasonAsync(int season, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Matches
                .Where(m => m.Season == season)
                .OrderBy(m => m.MatchDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get matches for season: {Season}", season);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<Match>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Matches
                .Where(m => m.MatchDate >= startDate && m.MatchDate <= endDate)
                .OrderBy(m => m.MatchDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get matches for date range: {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Matches.Add(match);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Added match: {MatchName} ({EspnId})", match.Name, match.EspnId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add match: {MatchName} ({EspnId})", match.Name, match.EspnId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Match match, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Matches.Update(match);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Updated match: {MatchName} ({EspnId})", match.Name, match.EspnId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update match: {MatchName} ({EspnId})", match.Name, match.EspnId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AddOrUpdateRangeAsync(IEnumerable<Match> matches, CancellationToken cancellationToken = default)
    {
        var matchList = matches.ToList();
        if (!matchList.Any())
            return;

        try
        {
            _logger.LogInformation("Processing {Count} matches for add/update", matchList.Count);

            // Get all ESPN IDs for the matches we're processing
            var espnIds = matchList.Select(m => m.EspnId).ToList();
            
            // Load existing matches with proper tracking for updates
            var existingMatches = await _context.Matches
                .Where(m => espnIds.Contains(m.EspnId))
                .ToDictionaryAsync(m => m.EspnId, cancellationToken);

            var matchesToAdd = new List<Match>();
            var updatedCount = 0;

            // Process each match - either add new or update existing
            foreach (var match in matchList)
            {
                if (existingMatches.TryGetValue(match.EspnId, out var existingMatch))
                {
                    // Update existing tracked entity
                    existingMatch.UpdateMatch(
                        match.Name,
                        match.MatchDate,
                        match.Status,
                        match.IsCompleted,
                        match.HomeScore,
                        match.AwayScore,
                        match.Venue
                    );
                    
                    // Update season type information that may have changed
                    existingMatch.Season = match.Season;
                    existingMatch.SeasonType = match.SeasonType;
                    existingMatch.SeasonTypeSlug = match.SeasonTypeSlug;
                    existingMatch.Week = match.Week;
                    
                    updatedCount++;
                }
                else
                {
                    // Add new match
                    matchesToAdd.Add(match);
                }
            }

            // Add new matches to context
            if (matchesToAdd.Any())
            {
                await _context.Matches.AddRangeAsync(matchesToAdd, cancellationToken);
            }

            // Save all changes in single transaction
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully processed {TotalCount} matches ({AddCount} added, {UpdateCount} updated)", 
                matchList.Count, matchesToAdd.Count, updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add or update {Count} matches", matchList.Count);
            throw;
        }
    }
}