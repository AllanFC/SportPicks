using FluentAssertions;
using Infrastructure.ExternalApis.Espn.Dtos;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using MatchEntity = Domain.Sports.Match;

namespace SportPicks.Tests.Infrastructure.Services;

/// <summary>
/// Unit tests for NflDataSyncService
/// </summary>
public class NflDataSyncServiceTests
{
    private readonly Mock<IEspnApiClient> _mockEspnApiClient;
    private readonly Mock<ITeamRepository> _mockTeamRepository;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<INflSeasonService> _mockSeasonService;
    private readonly Mock<ILogger<NflDataSyncService>> _mockLogger;
    private readonly NflSyncSettings _settings;
    private readonly NflDataSyncService _service;

    public NflDataSyncServiceTests()
    {
        _mockEspnApiClient = new Mock<IEspnApiClient>();
        _mockTeamRepository = new Mock<ITeamRepository>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockSeasonService = new Mock<INflSeasonService>();
        _mockLogger = new Mock<ILogger<NflDataSyncService>>();
        
        _settings = new NflSyncSettings
        {
            TargetSeason = 2024,
            TimeoutSeconds = 30,
            MaxRetries = 3,
            RetryDelayMs = 1000,
            ScoreboardLimit = 1000,
            DaysBack = 30,
            DaysForward = 365
        };
        
        var mockSettings = new Mock<IOptions<NflSyncSettings>>();
        mockSettings.Setup(s => s.Value).Returns(_settings);

        _service = new NflDataSyncService(
            _mockEspnApiClient.Object,
            _mockTeamRepository.Object,
            _mockMatchRepository.Object,
            _mockSeasonService.Object,
            _mockLogger.Object,
            mockSettings.Object);
    }

    [Fact]
    public async Task SyncTeamsAsync_WithValidResponse_ShouldReturnTeamCount()
    {
        // Arrange
        var teamsJson = CreateMockTeamsJson();
        _mockEspnApiClient.Setup(x => x.GetTeamsJsonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamsJson);

        _mockTeamRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Team>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SyncTeamsAsync();

        // Assert
        result.Should().Be(2);
        _mockTeamRepository.Verify(x => x.AddOrUpdateRangeAsync(
            It.Is<IEnumerable<Team>>(teams => teams.Count() == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncTeamsAsync_WithNullResponse_ShouldReturnZero()
    {
        // Arrange
        _mockEspnApiClient.Setup(x => x.GetTeamsJsonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.SyncTeamsAsync();

        // Assert
        result.Should().Be(0);
        _mockTeamRepository.Verify(x => x.AddOrUpdateRangeAsync(
            It.IsAny<IEnumerable<Team>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncMatchesAsync_WithValidResponse_ShouldReturnMatchCount()
    {
        // Arrange
        var scoreboardJson = CreateMockScoreboardJson();
        _mockEspnApiClient.Setup(x => x.GetScoreboardJsonAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(scoreboardJson);

        _mockSeasonService.Setup(x => x.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2024);

        _mockMatchRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<MatchEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SyncMatchesAsync();

        // Assert
        result.Should().Be(1);
        _mockMatchRepository.Verify(x => x.AddOrUpdateRangeAsync(
            It.Is<IEnumerable<MatchEntity>>(matches => matches.Count() == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncMatchesAsync_WithCustomDateRange_ShouldUseProvidedDates()
    {
        // Arrange
        var startDate = new DateTime(2024, 9, 1);
        var endDate = new DateTime(2024, 9, 30);
        var scoreboardJson = CreateMockScoreboardJson();
        
        _mockEspnApiClient.Setup(x => x.GetScoreboardJsonAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scoreboardJson);

        _mockMatchRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<MatchEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SyncMatchesAsync(startDate, endDate);

        // Assert
        result.Should().Be(1);
        _mockEspnApiClient.Verify(x => x.GetScoreboardJsonAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformFullSyncAsync_ShouldSyncBothTeamsAndMatches()
    {
        // Arrange
        var teamsJson = CreateMockTeamsJson();
        var scoreboardJson = CreateMockScoreboardJson();

        _mockEspnApiClient.Setup(x => x.GetTeamsJsonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamsJson);
        _mockEspnApiClient.Setup(x => x.GetScoreboardJsonAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(scoreboardJson);

        _mockSeasonService.Setup(x => x.GetCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2024);

        _mockTeamRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Team>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMatchRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<MatchEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var (teamsSynced, matchesSynced) = await _service.PerformFullSyncAsync();

        // Assert
        teamsSynced.Should().Be(2);
        matchesSynced.Should().Be(1);
        
        _mockTeamRepository.Verify(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Team>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMatchRepository.Verify(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<MatchEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncMatchesForSeasonAsync_ShouldUseSeasonService()
    {
        // Arrange
        var season = 2023;
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 2, 28);
        var scoreboardJson = CreateMockScoreboardJson();

        _mockSeasonService.Setup(x => x.GetSeasonDateRangeAsync(season, It.IsAny<CancellationToken>()))
            .ReturnsAsync((startDate, endDate));
        
        _mockEspnApiClient.Setup(x => x.GetScoreboardJsonAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scoreboardJson);

        _mockMatchRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<MatchEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SyncMatchesForSeasonAsync(season);

        // Assert
        result.Should().Be(1);
        _mockSeasonService.Verify(x => x.GetSeasonDateRangeAsync(season, It.IsAny<CancellationToken>()), Times.Once);
        _mockEspnApiClient.Verify(x => x.GetScoreboardJsonAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static string CreateMockTeamsJson()
    {
        return """
        {
            "sports": [
                {
                    "leagues": [
                        {
                            "teams": [
                                {
                                    "team": {
                                        "id": "1",
                                        "displayName": "New England Patriots",
                                        "abbreviation": "NE",
                                        "location": "New England",
                                        "nickname": "Patriots",
                                        "color": "#002244",
                                        "isActive": true,
                                        "logos": [
                                            {
                                                "href": "https://example.com/patriots-logo.png"
                                            }
                                        ]
                                    }
                                },
                                {
                                    "team": {
                                        "id": "2",
                                        "displayName": "Buffalo Bills",
                                        "abbreviation": "BUF",
                                        "location": "Buffalo",
                                        "nickname": "Bills",
                                        "color": "#00338D",
                                        "isActive": true,
                                        "logos": [
                                            {
                                                "href": "https://example.com/bills-logo.png"
                                            }
                                        ]
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }
        """;
    }

    private static string CreateMockScoreboardJson()
    {
        return """
        {
            "season": {
                "year": 2024
            },
            "week": {
                "number": 1
            },
            "events": [
                {
                    "id": "401547417",
                    "name": "Patriots at Bills",
                    "date": "2024-09-15T17:00:00Z",
                    "status": {
                        "type": {
                            "state": "pre",
                            "completed": false
                        }
                    },
                    "competitions": [
                        {
                            "competitors": [
                                {
                                    "id": "1",
                                    "homeAway": "home",
                                    "team": {
                                        "id": "2",
                                        "displayName": "Buffalo Bills"
                                    }
                                },
                                {
                                    "id": "2",
                                    "homeAway": "away",
                                    "team": {
                                        "id": "1",
                                        "displayName": "New England Patriots"
                                    }
                                }
                            ],
                            "venue": {
                                "fullName": "Highmark Stadium"
                            }
                        }
                    ]
                }
            ]
        }
        """;
    }
}