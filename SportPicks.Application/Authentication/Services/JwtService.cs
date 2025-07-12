using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Application.Authentication.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<JwtService> _logger;

    public JwtService(ILogger<JwtService> logger, IOptions<JwtSettings> jwtSettings, IUserRepository userRepository)
    {
        _logger = logger;
        _jwtSettings = jwtSettings.Value; // Get the configuration values
        _userRepository = userRepository;
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserRole)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<(string JwtToken, string RefreshToken)> GenerateTokensAsync(User user)
    {
        var jwtToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateUserAsync(user);

        return (jwtToken, refreshToken);
    }

    public async Task<(string JwtToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
        if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        // Generate new tokens
        var tokens = await GenerateTokensAsync(user);

        // Update the user with the new refresh token
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Adjust expiry time
        await _userRepository.UpdateUserAsync(user);

        _logger.LogInformation("Refresh token refreshed for user: {UserId}", user.Id);

        return tokens;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);

        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1);

            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("Refresh token revoked for user: {UserId}", user.Id);
        }
    }
}
