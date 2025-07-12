# SportPicks Backend

## Tech Stack
- **.NET 9**
- **Entity Framework Core** (PostgreSQL provider)
- **PostgreSQL** for database
- **Argon2** for password hashing and salt (via Konscious.Security.Cryptography)
- **Serilog** for logging (with PostgreSQL sink)
- **JWT Authentication** (access and refresh tokens)
- **Vertical Slice Architecture** with Clean Architecture principles
- **xUnit, Moq, FluentAssertions** for testing

## Project Structure
- **SportPicks.API**: ASP.NET Core Web API, controllers for authentication and user management, configures DI, authentication, logging, and CORS.
- **SportPicks.Application**: Application logic, services (UserService, JwtService), interfaces, DTOs, and options (JwtSettings).
- **SportPicks.Infrastructure**: Data access (repositories, ApplicationDbContext), password hashing, EF Core migrations.
- **SportPicks.Domain**: Domain models (User), enums (UserRolesEnum).
- **SportPicks.Tests**: Unit and integration tests for services, repositories, and security.

## Features
- User registration and login (username/email + password)
- Password hashing and verification using Argon2
- JWT access and refresh token generation, validation, and revocation
- User password update endpoint
- Role-based user model (Admin, User)
- Logging to PostgreSQL via Serilog
- EF Core migrations for schema management
- Clean separation of concerns and testable code

## API Endpoints (v1)
- `POST /api/v1/auth/login` — User login, returns JWT and sets refresh token cookie
- `POST /api/v1/auth/logout` — Logout, revokes refresh token
- `POST /api/v1/auth/refresh` — Refresh JWT using refresh token cookie
- `POST /api/v1/users` — Register new user
- `PUT /api/v1/users/{userId}/password` — Update user password

## Database Migrations
Run these commands from the solution root:
dotnet ef migrations add <MigrationName> --project .\SportPicks.Infrastructure --startup-project .\SportPicks.API

dotnet ef database update --project .\SportPicks.Infrastructure --startup-project .\SportPicks.API

dotnet ef migrations remove --project .\SportPicks.Infrastructure --startup-project .\SportPicks.API

## Configuration
- **appsettings.json**: Set JWT Issuer, Audience, and (via secrets) Key.
- **User secrets**: Store sensitive JWT key and connection strings securely.

## Testing
- Run all tests:dotnet test
---

This project follows vertical slice and clean architecture principles for maintainability and scalability.