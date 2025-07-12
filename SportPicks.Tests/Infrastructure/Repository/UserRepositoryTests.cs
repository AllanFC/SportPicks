using Application.Common.Interfaces;
using Application.Users.Dtos;
using Application.Users.Services;
using Domain.Users;
using Microsoft.Extensions.Logging;
using Moq;

namespace SportPicks.Tests.Infrastructure.Repository;

using Microsoft.EntityFrameworkCore;
using global::Infrastructure.Persistence.Repositories;
using global::Infrastructure.Persistence;
using FluentAssertions;

public class UserRepositoryTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb").Options;
        _dbContext = new ApplicationDbContext(options);
        _userRepository = new UserRepository(_dbContext);
    }

    [Fact]
    public async Task AddUser_Should_Save_User_To_Database()
    {
        // Arrange
        var user = new User("testuser", "test@test.dk", "/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=", "QKtM7ti/o8AdyZA3oNyvHw==");

        // Act
        await _userRepository.AddUserAsync(user);
        var retrievedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "testuser");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser.PasswordHash.Should().Be("/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=");
    }

    [Fact]
    public async Task GetUserById_Should_Retrieve_User_From_Database()
    {
        // Arrange
        var user = new User("testuser", "test@test.dk", "/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=", "QKtM7ti/o8AdyZA3oNyvHw==");

        // Act
        await _userRepository.AddUserAsync(user);
        var retrievedUser = await _userRepository.GetUserByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetUserByUsername_Should_Retrieve_User_From_Database()
    {
        // Arrange
        var user = new User("testuser", "test@test.dk", "/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=", "QKtM7ti/o8AdyZA3oNyvHw==");

        // Act
        await _userRepository.AddUserAsync(user);
        var retrievedUser = await _userRepository.GetUserByUsernameAsync(user.Username);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetUserByEmail_Should_Retrieve_User_From_Database()
    {
        // Arrange
        var user = new User("testuser", "test@test.dk", "/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=", "QKtM7ti/o8AdyZA3oNyvHw==");

        // Act
        await _userRepository.AddUserAsync(user);
        var retrievedUser = await _userRepository.GetUserByEmailAsync(user.Email);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task IsEmailTaken_Should_Be_True()
    {
        // Arrange
        var user = new User("testuser", "test@test.dk", "/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=", "QKtM7ti/o8AdyZA3oNyvHw==");

        // Act
        await _userRepository.AddUserAsync(user);
        var IsTaken = await _userRepository.IsEmailTakenAsync("test@test.dk");

        // Assert
        IsTaken.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailTaken_Should_Be_False()
    {
        // Arrange
        var user = new User("testuser", "test@test.dk", "/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=", "QKtM7ti/o8AdyZA3oNyvHw==");

        // Act
        await _dbContext.Users.AddAsync(user);
        var IsTaken = await _userRepository.IsEmailTakenAsync("different@test.dk");

        // Assert
        IsTaken.Should().BeFalse();
    }

    [Fact]
    public async Task IsUsernameTaken_Should_Be_True()
    {
        // Arrange
        var user = new User("testuser", "test@test.dk", "/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=", "QKtM7ti/o8AdyZA3oNyvHw==");

        // Act
        await _dbContext.Users.AddAsync(user);
        var IsTaken = await _userRepository.IsUsernameTakenAsync("testuser");

        // Assert
        IsTaken.Should().BeTrue();
    }

    [Fact]
    public async Task IsUsernameTaken_Should_Be_False()
    {
        // Arrange
        var user = new User("testuser", "test@test.dk", "/+jwg5MXj3i36A7aTyAerGaFR3D/I2XQexjmuzAG530=", "QKtM7ti/o8AdyZA3oNyvHw==");

        // Act
        await _dbContext.Users.AddAsync(user);
        var IsTaken = await _userRepository.IsUsernameTakenAsync("differentUser2");

        // Assert
        IsTaken.Should().BeFalse();
    }
}
