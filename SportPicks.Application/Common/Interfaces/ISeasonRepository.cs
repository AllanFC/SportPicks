using Domain.Sports;

namespace Application.Common.Interfaces;

public interface ISeasonRepository
{
    Task<Season?> GetByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<Season?> GetByYearAndSportAsync(int year, Guid sportId, CancellationToken cancellationToken = default);
    Task<Season?> GetCurrentActiveSeasonAsync(CancellationToken cancellationToken = default);
    Task<Season?> GetActiveByYearAndSportAsync(int year, Guid sportId, CancellationToken cancellationToken = default);
    Task<List<Season>> GetAllSeasonsAsync(CancellationToken cancellationToken = default);
    Task<List<Season>> GetBySportAsync(Guid sportId, CancellationToken cancellationToken = default);
    Task AddAsync(Season season, CancellationToken cancellationToken = default);
    Task UpdateAsync(Season season, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(Season season, CancellationToken cancellationToken = default);
}