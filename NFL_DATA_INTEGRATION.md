# **NFL Data Integration with ESPN APIs**

## **What This Does**
- Syncs NFL team and match data from ESPN's official APIs
- Stores ESPN's exact season boundaries in database (no more hardcoded dates)
- Captures season types (preseason, regular season, postseason) and official week numbers
- Uses ESPN Core API for authoritative season information

## **Database Tables**
- **Teams**: ESPN team data (Patriots, Bills, etc.)
- **Matches**: Game data with scores, status, venue
- **Seasons**: Official ESPN season boundaries (2025: July 31 ? February 12)

## **ESPN APIs Used**
- **Teams API**: `site.api.espn.com/apis/site/v2/sports/football/nfl/teams`
- **Scoreboard API**: `site.api.espn.com/apis/site/v2/sports/football/nfl/scoreboard`
- **Core Season API**: `sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{year}`

## **Admin Endpoints**
- `POST /api/v1/admin/nfl-sync/teams` - Sync teams
- `POST /api/v1/admin/nfl-sync/matches` - Sync matches for date range
- `POST /api/v1/admin/nfl-sync/seasons/{year}/matches` - Sync matches for specific season
- `POST /api/v1/admin/nfl-sync/full` - Full sync (teams + matches)

## **How It Works**
1. **Season Sync**: Fetch official season data from ESPN Core API ? Store in database
2. **Team Sync**: Get all NFL teams ? Store/update in database  
3. **Match Sync**: Get games within exact season boundaries ? Store with official week numbers
4. **Smart Fallbacks**: Database ? ESPN API ? Simple estimate (only if both fail)

## **Example Season Data from ESPN**
```json
{
  "year": 2025,
  "startDate": "2025-07-31T07:00Z",
  "endDate": "2026-02-12T07:59Z",
  "types": [
    {"type": 1, "name": "Preseason", "slug": "preseason"},
    {"type": 2, "name": "Regular Season", "slug": "regular-season"},
    {"type": 3, "name": "Postseason", "slug": "post-season"}
  ]
}
```

This gets stored in the database and used for all sync operations - no more guessing dates!