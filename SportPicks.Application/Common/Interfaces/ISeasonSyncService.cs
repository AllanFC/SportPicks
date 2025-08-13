using Domain.Sports;

namespace Application.Common.Interfaces;

public interface ISeasonSyncService
{
    Task<Season?> SyncSeasonAsync(int year, CancellationToken cancellationToken = default);
    Task<Season?> SyncCurrentSeasonAsync(CancellationToken cancellationToken = default);
    Task<int> UpdateActiveSeasonStatusAsync(CancellationToken cancellationToken = default);
}