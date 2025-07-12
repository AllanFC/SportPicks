namespace Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    Task<(string JwtToken, string RefreshToken)> GenerateTokensAsync(User user);
    Task<(string JwtToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
