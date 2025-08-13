using FluentAssertions;
using Infrastructure.ExternalApis.Espn.Dtos;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Application.Common.Interfaces;
using Domain.Sports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SportPicks.Tests.Infrastructure.Services;

/// <summary>
/// Unit tests for NflDataSyncService
/// </summary>
public sealed class NflDataSyncServiceTests : IDisposable
{
    private readonly Mock<IEspnApiClient> _mockEspnApiClient;
    private readonly Mock<ICompetitorRepository> _mockCompetitorRepository;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ISeasonRepository> _mockSeasonRepository;
    private readonly Mock<ISeasonSyncService> _mockSeasonSyncService;
    private readonly Mock<ILogger<NflDataSyncService>> _mockLogger;
    private readonly Mock<IOptions<NflSyncSettings>> _mockSettings;
    private readonly ApplicationDbContext _context;
    private readonly NflDataSyncService _service;

    // NFL sport ID for testing - matches the actual service
    private static readonly Guid NflSportId = new("11111111-1111-1111-1111-111111111111");

    public NflDataSyncServiceTests()
    {
        _mockEspnApiClient = new Mock<IEspnApiClient>();
        _mockCompetitorRepository = new Mock<ICompetitorRepository>();
        _mockEventRepository = new Mock<IEventRepository>();
        _mockSeasonRepository = new Mock<ISeasonRepository>();
        _mockSeasonSyncService = new Mock<ISeasonSyncService>();
        _mockLogger = new Mock<ILogger<NflDataSyncService>>();
        
        // Create real in-memory database context since it's sealed
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        
        var settings = new NflSyncSettings
        {
            BaseUrl = "https://site.api.espn.com",
            TargetSeason = 2024,
            TimeoutSeconds = 30,
            MaxRetries = 3,
            RetryDelayMs = 1000,
            ScoreboardLimit = 1000
        };

        _mockSettings = new Mock<IOptions<NflSyncSettings>>();
        _mockSettings.Setup(x => x.Value).Returns(settings);

        // Add NFL sport to in-memory database for EnsureNflSportExistsAsync
        _context.Sports.Add(new Sport("National Football League", "NFL")
        {
            Id = NflSportId,
            Description = "American professional football league",
            IsActive = true
        });
        _context.SaveChanges();

        _service = new NflDataSyncService(
            _mockEspnApiClient.Object,
            _mockCompetitorRepository.Object,
            _mockEventRepository.Object,
            _mockSeasonRepository.Object,
            _mockSeasonSyncService.Object,
            _context,
            _mockLogger.Object,
            _mockSettings.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task SyncTeamsAsync_WithValidResponse_ShouldReturnTeamCount()
    {
        // Arrange
        var teamsJson = CreateMockTeamsJson();
        _mockEspnApiClient.Setup(x => x.GetTeamsJsonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamsJson);

        _mockCompetitorRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Competitor>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SyncTeamsAsync();

        // Assert
        result.Should().Be(2);
        _mockCompetitorRepository.Verify(x => x.AddOrUpdateRangeAsync(
            It.Is<IEnumerable<Competitor>>(competitors => competitors.Count() == 2),
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
        _mockCompetitorRepository.Verify(x => x.AddOrUpdateRangeAsync(
            It.IsAny<IEnumerable<Competitor>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncMatchesAsync_WithValidResponse_ShouldReturnMatchCount()
    {
        // Arrange
        var scoreboardJson = CreateMockScoreboardJson();
        var mockSeason = new Season(2024, "2024", NflSportId, DateTime.Now.AddMonths(-3), DateTime.Now.AddMonths(3), true);
        var homeCompetitor = new Competitor("Buffalo Bills", "BUF", NflSportId);
        var awayCompetitor = new Competitor("New England Patriots", "NE", NflSportId);
        
        _mockSeasonSyncService.Setup(x => x.SyncCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSeason);
        
        _mockSeasonRepository.Setup(x => x.GetByYearAndSportAsync(2024, NflSportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSeason);

        _mockCompetitorRepository.Setup(x => x.GetByExternalIdAsync("2", "ESPN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeCompetitor);
        
        _mockCompetitorRepository.Setup(x => x.GetByExternalIdAsync("1", "ESPN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(awayCompetitor);
        
        _mockEspnApiClient.Setup(x => x.GetScoreboardJsonAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(scoreboardJson);

        _mockEventRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Event>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SyncMatchesAsync();

        // Assert
        result.Should().Be(1);
        _mockEventRepository.Verify(x => x.AddOrUpdateRangeAsync(
            It.Is<IEnumerable<Event>>(events => events.Count() == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncMatchesAsync_WithCustomDateRange_ShouldUseProvidedDates()
    {
        // Arrange
        var startDate = new DateTime(2024, 9, 1);
        var endDate = new DateTime(2024, 9, 30);
        var scoreboardJson = CreateMockScoreboardJson();
        var mockSeason = new Season(2024, "2024", NflSportId, startDate, endDate, true);
        var homeCompetitor = new Competitor("Buffalo Bills", "BUF", NflSportId);
        var awayCompetitor = new Competitor("New England Patriots", "NE", NflSportId);

        _mockSeasonRepository.Setup(x => x.GetByYearAndSportAsync(2024, NflSportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSeason);

        _mockCompetitorRepository.Setup(x => x.GetByExternalIdAsync("2", "ESPN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeCompetitor);
        
        _mockCompetitorRepository.Setup(x => x.GetByExternalIdAsync("1", "ESPN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(awayCompetitor);
        
        _mockEspnApiClient.Setup(x => x.GetScoreboardJsonAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scoreboardJson);

        _mockEventRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Event>>(), It.IsAny<CancellationToken>()))
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
        var mockSeason = new Season(2024, "2024", NflSportId, DateTime.Now.AddMonths(-3), DateTime.Now.AddMonths(3), true);
        var homeCompetitor = new Competitor("Buffalo Bills", "BUF", NflSportId);
        var awayCompetitor = new Competitor("New England Patriots", "NE", NflSportId);

        _mockSeasonSyncService.Setup(x => x.SyncCurrentSeasonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSeason);

        _mockSeasonRepository.Setup(x => x.GetByYearAndSportAsync(2024, NflSportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSeason);

        _mockCompetitorRepository.Setup(x => x.GetByExternalIdAsync("2", "ESPN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeCompetitor);
        
        _mockCompetitorRepository.Setup(x => x.GetByExternalIdAsync("1", "ESPN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(awayCompetitor);

        _mockEspnApiClient.Setup(x => x.GetTeamsJsonAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamsJson);
        _mockEspnApiClient.Setup(x => x.GetScoreboardJsonAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(scoreboardJson);

        _mockCompetitorRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Competitor>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockEventRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Event>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var (teamsSynced, matchesSynced) = await _service.PerformFullSyncAsync();

        // Assert
        teamsSynced.Should().Be(2);
        matchesSynced.Should().Be(1);
        
        _mockCompetitorRepository.Verify(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Competitor>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventRepository.Verify(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Event>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncMatchesForSeasonAsync_ShouldUseSeasonRepository()
    {
        // Arrange
        var season = 2024; // Changed from 2023 to match the mock JSON data
        var startDate = new DateTime(2024, 9, 1);
        var endDate = new DateTime(2024, 12, 31);
        var scoreboardJson = CreateMockScoreboardJson();
        var mockSeasonEntity = new Season(season, "2024", NflSportId, startDate, endDate, true);
        var homeCompetitor = new Competitor("Buffalo Bills", "BUF", NflSportId);
        var awayCompetitor = new Competitor("New England Patriots", "NE", NflSportId);

        _mockSeasonRepository.Setup(x => x.GetByYearAndSportAsync(season, NflSportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSeasonEntity);

        _mockCompetitorRepository.Setup(x => x.GetByExternalIdAsync("2", "ESPN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeCompetitor);
        
        _mockCompetitorRepository.Setup(x => x.GetByExternalIdAsync("1", "ESPN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(awayCompetitor);
        
        _mockEspnApiClient.Setup(x => x.GetScoreboardJsonAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scoreboardJson);

        _mockEventRepository.Setup(x => x.AddOrUpdateRangeAsync(It.IsAny<IEnumerable<Event>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SyncMatchesForSeasonAsync(season);

        // Assert
        result.Should().Be(1);
        // Verify season repository was called (it may be called multiple times internally)
        _mockSeasonRepository.Verify(x => x.GetByYearAndSportAsync(season, NflSportId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
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
                "year": 2024,
                "type": 2,
                "slug": "regular"
            },
            "week": {
                "number": 1
            },
            "events": [
                {
                    "id": "401547417",
                    "name": "Patriots at Bills",
                    "date": "2024-09-15T17:00:00Z",
                    "season": {
                        "year": 2024,
                        "type": 2,
                        "slug": "regular"
                    },
                    "week": {
                        "number": 1
                    },
                    "status": {
                        "type": {
                            "state": "post",
                            "completed": true
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
                                    },
                                    "score": "24",
                                    "winner": true
                                },
                                {
                                    "id": "2",
                                    "homeAway": "away",
                                    "team": {
                                        "id": "1",
                                        "displayName": "New England Patriots"
                                    },
                                    "score": "17",
                                    "winner": false
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