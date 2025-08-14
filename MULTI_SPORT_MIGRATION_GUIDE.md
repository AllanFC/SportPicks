# Multi-Sport Pick'ems Schema Migration Guide

This document provides a comprehensive guide for migrating from the single-sport NFL schema to the new multi-sport pick'ems schema.

## Overview

The new schema supports multiple sports with the following key changes:

1. **Sport Entity**: Central entity for different sports (NFL, F1, NBA, etc.)
2. **Competitor Entity**: Unified entity for teams and individual competitors
3. **Event Entity**: Replaces Match, supports various event types
4. **EventCompetitor**: Junction table linking events to participants
5. **League System**: Full league management with public/private leagues
6. **Pick System**: Supports both simple picks and ranked predictions

## Migration Process

### 1. Database Schema Migration

Run the migration to update your database schema:

```bash
dotnet ef database update --project .\SportPicks.Infrastructure --startup-project .\SportPicks.API
```

This migration will:
- Create all new multi-sport tables
- Preserve existing NFL data by migrating:
  - Teams ? Competitors
  - Matches ? Events + EventCompetitors
  - Seasons ? Seasons (with SportId)
- Create proper indexes for optimal query performance

### 2. Data Seeding

After migration, seed initial sports data:

```csharp
// In your startup or admin service
var dataSeedService = serviceProvider.GetRequiredService<IDataSeedService>();
await dataSeedService.SeedInitialSportsAsync();
await dataSeedService.SeedNflOfficialLeaguesAsync();
```

## Key Entity Relationships

### Sport ? Season ? Event ? EventCompetitor
```
Sport (NFL)
??? Season (2024 NFL Season)
    ??? Event (Patriots vs Bills)
        ??? EventCompetitor (Patriots, away, score: 14)
        ??? EventCompetitor (Bills, home, score: 21)
```

### League ? LeagueMember ? Pick
```
League (NFL Official Pick'em)
??? LeagueMember (User1, 150 points)
??? Pick (User1 picks Bills to win vs Patriots)
```

## Query Examples

### Get User's Picks Across All Sports and Leagues

```csharp
var userPicks = await context.Picks
    .Include(p => p.League)
        .ThenInclude(l => l.Sport)
    .Include(p => p.Event)
        .ThenInclude(e => e.Season)
    .Include(p => p.PickedCompetitor)
    .Where(p => p.UserId == userId)
    .OrderByDescending(p => p.PickedAt)
    .ToListAsync();
```

### Get League Leaderboard

```csharp
var leaderboard = await context.LeagueMembers
    .Include(lm => lm.User)
    .Where(lm => lm.LeagueId == leagueId && lm.IsActive)
    .OrderByDescending(lm => lm.TotalPoints)
    .ThenByDescending(lm => lm.CorrectPicks)
    .Select(lm => new LeaderboardEntry
    {
        Username = lm.User.Username,
        TotalPoints = lm.TotalPoints,
        CorrectPicks = lm.CorrectPicks,
        TotalPicks = lm.TotalPicks,
        Percentage = lm.TotalPicks > 0 ? (double)lm.CorrectPicks / lm.TotalPicks * 100 : 0
    })
    .ToListAsync();
```

### Get Upcoming Events for a Sport

```csharp
var upcomingEvents = await context.Events
    .Include(e => e.Season)
        .ThenInclude(s => s.Sport)
    .Include(e => e.EventCompetitors)
        .ThenInclude(ec => ec.Competitor)
    .Where(e => e.Season.Sport.Code == "NFL" && 
                e.EventDate > DateTime.UtcNow && 
                !e.IsCompleted)
    .OrderBy(e => e.EventDate)
    .Take(20)
    .ToListAsync();
```

### Get User's Leagues by Sport

```csharp
var userLeagues = await context.LeagueMembers
    .Include(lm => lm.League)
        .ThenInclude(l => l.Sport)
    .Where(lm => lm.UserId == userId && lm.IsActive)
    .GroupBy(lm => lm.League.Sport.Code)
    .Select(g => new SportLeagues
    {
        SportCode = g.Key,
        SportName = g.First().League.Sport.Name,
        Leagues = g.Select(lm => new LeagueInfo
        {
            Id = lm.League.Id,
            Name = lm.League.Name,
            IsPublic = lm.League.IsPublic,
            MemberCount = lm.League.LeagueMembers.Count(m => m.IsActive),
            UserRank = lm.TotalPoints // You'd need to calculate actual rank
        }).ToList()
    })
    .ToListAsync();
```

## Migrating Existing Code

### Old NFL-specific code:
```csharp
// OLD: NFL-specific
var nflMatches = await context.Matches
    .Where(m => m.Season == 2024)
    .ToListAsync();
```

### New multi-sport code:
```csharp
// NEW: Multi-sport
var nflEvents = await context.Events
    .Include(e => e.Season)
        .ThenInclude(s => s.Sport)
    .Where(e => e.Season.Sport.Code == "NFL" && e.Season.Year == 2024)
    .ToListAsync();
```

## Performance Considerations

The new schema includes comprehensive indexing:

- **Composite indexes** for common query patterns (SportId + Year, LeagueId + TotalPoints)
- **Foreign key indexes** for all relationships
- **Unique constraints** where appropriate (Sport.Code, League.InviteCode)

## Backward Compatibility

During the transition period, both old and new tables exist:
- Legacy `Teams` and `Matches` tables remain for compatibility
- New services should use `Competitors`, `Events`, and `EventCompetitors`
- Old services can continue using legacy tables until fully migrated

## Next Steps

1. **Update Services**: Migrate existing services to use new entities
2. **Create League Management**: Build UI/API for league creation and management
3. **Implement Pick System**: Add pick creation and scoring logic
4. **Add New Sports**: Extend to support F1, NBA, or other sports
5. **Remove Legacy Tables**: Once fully migrated, drop old tables

## Sport-Specific Considerations

### NFL
- Uses `Week` field in Events
- Home/Away teams via `IsHomeTeam` in EventCompetitors
- Simple winner picks in Pick table

### Formula 1 (Future)
- Uses `Round` field in Events
- Individual competitors (drivers)
- Ranked picks for top-10 predictions

### NBA (Future)
- Similar to NFL but with different season structure
- Could support quarter-by-quarter scoring

This schema provides a solid foundation for a comprehensive multi-sport pick'ems platform while preserving your existing NFL data.