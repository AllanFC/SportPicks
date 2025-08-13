using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportPicks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Salt = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Competitors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AlternateColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalSource = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Competitors_Sport_SportId",
                        column: x => x.SportId,
                        principalTable: "Sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Sport_SportId",
                        column: x => x.SportId,
                        principalTable: "Sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SportId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsOfficial = table.Column<bool>(type: "boolean", nullable: false),
                    InviteCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MaxMembers = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leagues_Sport_SportId",
                        column: x => x.SportId,
                        principalTable: "Sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Leagues_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Week = table.Column<int>(type: "integer", nullable: true),
                    Round = table.Column<int>(type: "integer", nullable: true),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalSource = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeagueMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TotalPoints = table.Column<int>(type: "integer", nullable: false),
                    CorrectPicks = table.Column<int>(type: "integer", nullable: false),
                    TotalPicks = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueMembers_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueMembers_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventCompetitors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsHomeTeam = table.Column<bool>(type: "boolean", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: true),
                    IsWinner = table.Column<bool>(type: "boolean", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCompetitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventCompetitors_Competitor_CompetitorId",
                        column: x => x.CompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventCompetitors_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Picks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    PickedCompetitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    PickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Picks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Picks_Competitor_PickedCompetitorId",
                        column: x => x.PickedCompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Picks_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Picks_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Picks_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RankedPicks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalPoints = table.Column<int>(type: "integer", nullable: false),
                    PickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankedPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankedPicks_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RankedPicks_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RankedPicks_User_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RankedPickDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RankedPickId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictedPosition = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    ActualPosition = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankedPickDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankedPickDetails_Competitor_CompetitorId",
                        column: x => x.CompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RankedPickDetails_RankedPick_RankedPickId",
                        column: x => x.RankedPickId,
                        principalTable: "RankedPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_Code",
                table: "Competitors",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_ExternalId",
                table: "Competitors",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_ExternalSource_ExternalId",
                table: "Competitors",
                columns: new[] { "ExternalSource", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_IsActive",
                table: "Competitors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_SportId",
                table: "Competitors",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_SportId_Code",
                table: "Competitors",
                columns: new[] { "SportId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventCompetitors_CompetitorId",
                table: "EventCompetitors",
                column: "CompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_EventCompetitors_EventId",
                table: "EventCompetitors",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventCompetitors_EventId_CompetitorId",
                table: "EventCompetitors",
                columns: new[] { "EventId", "CompetitorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventDate",
                table: "Events",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ExternalId",
                table: "Events",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ExternalSource_ExternalId",
                table: "Events",
                columns: new[] { "ExternalSource", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsCompleted",
                table: "Events",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Round",
                table: "Events",
                column: "Round");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SeasonId",
                table: "Events",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SeasonId_EventDate",
                table: "Events",
                columns: new[] { "SeasonId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_SeasonId_Week",
                table: "Events",
                columns: new[] { "SeasonId", "Week" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_Week",
                table: "Events",
                column: "Week");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_IsActive",
                table: "LeagueMembers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_LeagueId",
                table: "LeagueMembers",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_LeagueId_TotalPoints",
                table: "LeagueMembers",
                columns: new[] { "LeagueId", "TotalPoints" });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_UserId",
                table: "LeagueMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_UserId_LeagueId",
                table: "LeagueMembers",
                columns: new[] { "UserId", "LeagueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_CreatedByUserId",
                table: "Leagues",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_InviteCode",
                table: "Leagues",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_IsActive",
                table: "Leagues",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_IsPublic",
                table: "Leagues",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_SportId",
                table: "Leagues",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_Picks_EventId",
                table: "Picks",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Picks_LeagueId",
                table: "Picks",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Picks_LeagueId_EventId",
                table: "Picks",
                columns: new[] { "LeagueId", "EventId" });

            migrationBuilder.CreateIndex(
                name: "IX_Picks_PickedCompetitorId",
                table: "Picks",
                column: "PickedCompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Picks_UserId",
                table: "Picks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Picks_UserId_LeagueId_EventId",
                table: "Picks",
                columns: new[] { "UserId", "LeagueId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankedPickDetails_CompetitorId",
                table: "RankedPickDetails",
                column: "CompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_RankedPickDetails_RankedPickId",
                table: "RankedPickDetails",
                column: "RankedPickId");

            migrationBuilder.CreateIndex(
                name: "IX_RankedPickDetails_RankedPickId_CompetitorId",
                table: "RankedPickDetails",
                columns: new[] { "RankedPickId", "CompetitorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankedPickDetails_RankedPickId_PredictedPosition",
                table: "RankedPickDetails",
                columns: new[] { "RankedPickId", "PredictedPosition" });

            migrationBuilder.CreateIndex(
                name: "IX_RankedPicks_EventId",
                table: "RankedPicks",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_RankedPicks_LeagueId",
                table: "RankedPicks",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_RankedPicks_UserId",
                table: "RankedPicks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RankedPicks_UserId_LeagueId_EventId",
                table: "RankedPicks",
                columns: new[] { "UserId", "LeagueId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_EndDate",
                table: "Seasons",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_IsActive",
                table: "Seasons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_SportId",
                table: "Seasons",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_SportId_Year",
                table: "Seasons",
                columns: new[] { "SportId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_StartDate",
                table: "Seasons",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_StartDate_EndDate",
                table: "Seasons",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_Year",
                table: "Seasons",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_Sports_Code",
                table: "Sports",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sports_IsActive",
                table: "Sports",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Provider",
                table: "Users",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRole",
                table: "Users",
                column: "UserRole");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventCompetitors");

            migrationBuilder.DropTable(
                name: "LeagueMembers");

            migrationBuilder.DropTable(
                name: "Picks");

            migrationBuilder.DropTable(
                name: "RankedPickDetails");

            migrationBuilder.DropTable(
                name: "Competitors");

            migrationBuilder.DropTable(
                name: "RankedPicks");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Sports");
        }
    }
}
