using Azunt.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azunt.BackgroundCheckManagement;

/// <summary>
/// BackgroundCheck 테이블에 대한 Entity Framework Core 기반 리포지토리 구현체입니다.
/// </summary>
public class BackgroundCheckRepository : IBackgroundCheckRepository
{
    private readonly BackgroundCheckDbContextFactory _factory;
    private readonly ILogger<BackgroundCheckRepository> _logger;
    private readonly string? _connectionString;

    public BackgroundCheckRepository(
        BackgroundCheckDbContextFactory factory,
        ILoggerFactory loggerFactory)
    {
        _factory = factory;
        _logger = loggerFactory.CreateLogger<BackgroundCheckRepository>();
    }

    public BackgroundCheckRepository(
        BackgroundCheckDbContextFactory factory,
        ILoggerFactory loggerFactory,
        string connectionString)
    {
        _factory = factory;
        _logger = loggerFactory.CreateLogger<BackgroundCheckRepository>();
        _connectionString = connectionString;
    }

    private BackgroundCheckDbContext CreateContext() =>
        string.IsNullOrWhiteSpace(_connectionString)
            ? _factory.CreateDbContext()
            : _factory.CreateDbContext(_connectionString);

    public async Task<BackgroundCheck> AddAsync(BackgroundCheck model)
    {
        await using var context = CreateContext();
        model.CreatedAt = DateTimeOffset.UtcNow;

        context.BackgroundChecks.Add(model);
        await context.SaveChangesAsync();
        return model;
    }

    public async Task<IEnumerable<BackgroundCheck>> GetAllAsync()
    {
        await using var context = CreateContext();
        return await context.BackgroundChecks
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<BackgroundCheck> GetByIdAsync(long id)
    {
        await using var context = CreateContext();
        return await context.BackgroundChecks
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? new BackgroundCheck();
    }

    public async Task<bool> UpdateAsync(BackgroundCheck model)
    {
        await using var context = CreateContext();
        context.Attach(model);
        context.Entry(model).State = EntityState.Modified;
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await using var context = CreateContext();
        var entity = await context.BackgroundChecks.FindAsync(id);
        if (entity == null) return false;

        context.BackgroundChecks.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<ArticleSet<BackgroundCheck, int>> GetAllAsync<TParentIdentifier>(
        int pageIndex,
        int pageSize,
        string searchField,
        string searchQuery,
        string sortOrder,
        TParentIdentifier parentIdentifier,
        string category = "")
    {
        await using var context = CreateContext();

        var query = context.BackgroundChecks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery) &&
            (searchField?.Equals("Provider", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            query = query.Where(x => x.Provider != null && x.Provider.Contains(searchQuery));
        }

        query = sortOrder switch
        {
            "Id" => query.OrderBy(x => x.Id),
            "IdDesc" => query.OrderByDescending(x => x.Id),
            "CreatedAt" => query.OrderBy(x => x.CreatedAt),
            "CreatedAtDesc" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.Id)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ArticleSet<BackgroundCheck, int>(items, totalCount);
    }

    public Task<bool> MoveUpAsync(long id)
    {
        // DisplayOrder 기능이 없으므로 구현 생략
        return Task.FromResult(false);
    }

    public Task<bool> MoveDownAsync(long id)
    {
        // DisplayOrder 기능이 없으므로 구현 생략
        return Task.FromResult(false);
    }
}
