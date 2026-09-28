using PaintTint.Core.DTOs;

namespace PaintTint.Core.Services;

public interface IShadeService
{
    Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default);
}

public interface IBaseService
{
    Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default);
}

public interface IDispenseService
{
    Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default);
    Task<DispenseJobResponse> CreateDispenseJobAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default);
    Task<List<DispenseJobResponse>> GetRecentJobsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<DispenseJobResponse?> GetLatestJobAsync(CancellationToken cancellationToken = default);
}
