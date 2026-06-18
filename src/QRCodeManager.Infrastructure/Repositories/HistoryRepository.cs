using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Domain.Entities;
using QRCodeManager.Infrastructure.Data;

namespace QRCodeManager.Infrastructure.Repositories;

public class HistoryRepository : IHistoryRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<HistoryRepository> _logger;

    public HistoryRepository(AppDbContext dbContext, ILogger<HistoryRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<QrHistoryDto> AddAsync(QrHistoryDto history, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(history);
        _dbContext.QrHistories.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<IReadOnlyList<QrHistoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.QrHistories
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    public async Task<QrHistoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.QrHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.QrHistories.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return;
        }

        _dbContext.QrHistories.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<QrHistoryDto> UpdateAsync(QrHistoryDto history, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.QrHistories.FindAsync([history.Id], cancellationToken)
            ?? throw new InvalidOperationException($"Kayıt bulunamadı: {history.Id}");

        entity.Title = history.Title;
        entity.JsonData = history.JsonData;
        entity.QrImagePath = history.QrImagePath;
        entity.QrType = history.QrType;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    private static QrHistory MapToEntity(QrHistoryDto dto) =>
        new()
        {
            Id = dto.Id,
            Title = dto.Title,
            JsonData = dto.JsonData,
            QrImagePath = dto.QrImagePath,
            CreatedDate = dto.CreatedDate == default ? DateTime.UtcNow : dto.CreatedDate,
            QrType = dto.QrType
        };

    private static QrHistoryDto MapToDto(QrHistory entity)
    {
        byte[]? preview = null;
        if (!string.IsNullOrWhiteSpace(entity.QrImagePath) && File.Exists(entity.QrImagePath))
        {
            try
            {
                preview = File.ReadAllBytes(entity.QrImagePath);
            }
            catch
            {
                // Preview is optional.
            }
        }

        return new QrHistoryDto
        {
            Id = entity.Id,
            Title = entity.Title,
            JsonData = entity.JsonData,
            QrImagePath = entity.QrImagePath,
            CreatedDate = entity.CreatedDate,
            QrType = entity.QrType,
            PreviewImage = preview
        };
    }
}
