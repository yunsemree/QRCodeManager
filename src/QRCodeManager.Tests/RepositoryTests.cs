using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Domain.Enums;
using QRCodeManager.Infrastructure.Data;
using QRCodeManager.Infrastructure.Repositories;

namespace QRCodeManager.Tests;

public class RepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly HistoryRepository _sut;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
        _sut = new HistoryRepository(_dbContext, NullLogger<HistoryRepository>.Instance);
    }

    [Fact]
    public async Task AddAsync_PersistsHistory()
    {
        var dto = new QrHistoryDto
        {
            Title = "Test QR",
            JsonData = """{"name":"Test"}""",
            CreatedDate = DateTime.UtcNow,
            QrType = QrType.Generated
        };

        var result = await _sut.AddAsync(dto);
        var all = await _sut.GetAllAsync();

        Assert.True(result.Id > 0);
        Assert.Single(all);
        Assert.Equal("Test QR", all[0].Title);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRecord()
    {
        var created = await _sut.AddAsync(new QrHistoryDto
        {
            Title = "Old",
            JsonData = "{}",
            QrType = QrType.Generated
        });

        created.Title = "Updated";
        await _sut.UpdateAsync(created);

        var item = await _sut.GetByIdAsync(created.Id);
        Assert.Equal("Updated", item?.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRecord()
    {
        var created = await _sut.AddAsync(new QrHistoryDto
        {
            Title = "Delete Me",
            JsonData = "{}",
            QrType = QrType.Generated
        });

        await _sut.DeleteAsync(created.Id);
        var all = await _sut.GetAllAsync();

        Assert.Empty(all);
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }
}
