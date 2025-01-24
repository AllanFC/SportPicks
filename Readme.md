Tech used
- Argon for hashing and salt of password
- Entity Framework Core for ORM
- Postgres for database
- Vertical slice architecture with clean architecture principles


Entity Framework Core


dotnet ef migrations add InitialCreate --project .\SportPicks.Infrastructure --startup-project .\SportPicks.API

dotnet ef database update --project .\SportPicks.Infrastructure

dotnet ef migrations remove --project .\SportPicks.Infrastructure --startup-project .\SportPicks.API