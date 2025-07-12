using Application.Common.Interfaces;
using Application.Users.Services;
using Domain.Users;
using Microsoft.Extensions.Logging;
using Moq;

namespace SportPicks.Tests.Application.Services;

public class UserServiceTests
{
    private readonly UserService _userService;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_loggerMock.Object, _userRepoMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task RegisterUser_Should_Create_User()
    {
        // Arrange
        var username = "testuser";
        var email = "test@test.dk";
        var password = "Secure123";
        _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns(("hashedPassword", "salt"));

        // Act
        await _userService.RegisterUserAsync(username, email, password);

        // Assert
        _userRepoMock.Verify(r => r.AddUserAsync(It.Is<User>(u => u.Username == "testuser" && u.PasswordHash == "hashedPassword")), Times.Once);
    }
}