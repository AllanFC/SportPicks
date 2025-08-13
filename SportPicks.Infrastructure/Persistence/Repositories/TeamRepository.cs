using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Team entity
/// </summary>
public class TeamRepository : ITeamRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TeamRepository> _logger;

    public TeamRepository(ApplicationDbContext context, ILogger<TeamRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Team?> GetByEspnIdAsync(string espnId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Teams
                .FirstOrDefaultAsync(t => t.EspnId == espnId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get team by ESPN ID: {EspnId}", espnId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<Team>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Teams
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayName)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all active teams");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Added team: {TeamName} ({EspnId})", team.DisplayName, team.EspnId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add team: {TeamName} ({EspnId})", team.DisplayName, team.EspnId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Updated team: {TeamName} ({EspnId})", team.DisplayName, team.EspnId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update team: {TeamName} ({EspnId})", team.DisplayName, team.EspnId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AddOrUpdateRangeAsync(IEnumerable<Team> teams, CancellationToken cancellationToken = default)
    {
        var teamList = teams.ToList();
        if (!teamList.Any())
            return;

        try
        {
            _logger.LogInformation("Processing {Count} teams for add/update", teamList.Count);

            // Get all ESPN IDs for the teams we're processing
            var espnIds = teamList.Select(t => t.EspnId).ToList();
            
            // Load existing teams with proper tracking for updates
            var existingTeams = await _context.Teams
                .Where(t => espnIds.Contains(t.EspnId))
                .ToDictionaryAsync(t => t.EspnId, cancellationToken);

            var teamsToAdd = new List<Team>();
            var updatedCount = 0;

            // Process each team - either add new or update existing
            foreach (var team in teamList)
            {
                if (existingTeams.TryGetValue(team.EspnId, out var existingTeam))
                {
                    // Update existing tracked entity
                    existingTeam.UpdateTeamInfo(
                        team.DisplayName,
                        team.Abbreviation,
                        team.Location,
                        team.Nickname,
                        team.LogoUrl,
                        team.Color,
                        team.AlternateColor
                    );
                    updatedCount++;
                }
                else
                {
                    // Add new team
                    teamsToAdd.Add(team);
                }
            }

            // Add new teams to context
            if (teamsToAdd.Any())
            {
                await _context.Teams.AddRangeAsync(teamsToAdd, cancellationToken);
            }

            // Save all changes in single transaction
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully processed {TotalCount} teams ({AddCount} added, {UpdateCount} updated)", 
                teamList.Count, teamsToAdd.Count, updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add or update {Count} teams", teamList.Count);
            throw;
        }
    }
}