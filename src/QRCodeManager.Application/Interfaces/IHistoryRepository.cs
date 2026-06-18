using QRCodeManager.Application.DTOs;

namespace QRCodeManager.Application.Interfaces;

public interface IHistoryRepository
{
    Task<QrHistoryDto> AddAsync(QrHistoryDto history, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QrHistoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<QrHistoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<QrHistoryDto> UpdateAsync(QrHistoryDto history, CancellationToken cancellationToken = default);
}
