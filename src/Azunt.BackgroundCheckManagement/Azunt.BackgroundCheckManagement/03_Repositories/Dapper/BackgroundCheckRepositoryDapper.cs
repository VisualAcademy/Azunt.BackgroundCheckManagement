using Azunt.Models.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Azunt.BackgroundCheckManagement;

public class BackgroundCheckRepositoryDapper : IBackgroundCheckRepository
{
    private readonly string _connectionString;
    private readonly ILogger<BackgroundCheckRepositoryDapper> _logger;

    public BackgroundCheckRepositoryDapper(string connectionString, ILoggerFactory loggerFactory)
    {
        _connectionString = connectionString;
        _logger = loggerFactory.CreateLogger<BackgroundCheckRepositoryDapper>();
    }

    private SqlConnection GetConnection() => new(_connectionString);

    public async Task<BackgroundCheck> AddAsync(BackgroundCheck model)
    {
        const string sql = @"
INSERT INTO BackgroundChecks (
    Active, BackgroundCheckId, BackgroundStatus, CompletedAt, CreatedAt, CreatedBy, EmployeeId, FileName,
    InvestigationId, PackageId, BillCodeId, Provider, ReportUrl, Score, Status, UpdatedAt, VendorId
)
OUTPUT INSERTED.Id
VALUES (
    @Active, @BackgroundCheckId, @BackgroundStatus, @CompletedAt, @CreatedAt, @CreatedBy, @EmployeeId, @FileName,
    @InvestigationId, @PackageId, @BillCodeId, @Provider, @ReportUrl, @Score, @Status, @UpdatedAt, @VendorId
)";

        model.CreatedAt = DateTimeOffset.UtcNow;

        using var conn = GetConnection();
        model.Id = await conn.ExecuteScalarAsync<long>(sql, model);
        return model;
    }

    public async Task<IEnumerable<BackgroundCheck>> GetAllAsync()
    {
        const string sql = "SELECT * FROM BackgroundChecks ORDER BY Id DESC";
        using var conn = GetConnection();
        return await conn.QueryAsync<BackgroundCheck>(sql);
    }

    public async Task<BackgroundCheck> GetByIdAsync(long id)
    {
        const string sql = "SELECT * FROM BackgroundChecks WHERE Id = @Id";
        using var conn = GetConnection();
        return await conn.QuerySingleOrDefaultAsync<BackgroundCheck>(sql, new { Id = id }) ?? new BackgroundCheck();
    }

    public async Task<bool> UpdateAsync(BackgroundCheck model)
    {
        const string sql = @"
UPDATE BackgroundChecks SET
    Active = @Active,
    BackgroundCheckId = @BackgroundCheckId,
    BackgroundStatus = @BackgroundStatus,
    CompletedAt = @CompletedAt,
    CreatedBy = @CreatedBy,
    EmployeeId = @EmployeeId,
    FileName = @FileName,
    InvestigationId = @InvestigationId,
    PackageId = @PackageId,
    BillCodeId = @BillCodeId,
    Provider = @Provider,
    ReportUrl = @ReportUrl,
    Score = @Score,
    Status = @Status,
    UpdatedAt = @UpdatedAt,
    VendorId = @VendorId
WHERE Id = @Id";

        using var conn = GetConnection();
        return await conn.ExecuteAsync(sql, model) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM BackgroundChecks WHERE Id = @Id";
        using var conn = GetConnection();
        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
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
        var items = new List<BackgroundCheck>();
        int totalCount = 0;

        using var conn = GetConnection();
        var parameters = new DynamicParameters();
        var where = "1=1";

        if (!string.IsNullOrWhiteSpace(searchQuery) && searchField == "Provider")
        {
            where += " AND Provider LIKE @SearchQuery";
            parameters.Add("@SearchQuery", $"%{searchQuery}%");
        }

        var orderBy = sortOrder switch
        {
            "CreatedAt" => "ORDER BY CreatedAt DESC",
            "Provider" => "ORDER BY Provider",
            _ => "ORDER BY Id DESC"
        };

        string countSql = $"SELECT COUNT(*) FROM BackgroundChecks WHERE {where}";
        string dataSql = $@"
SELECT * FROM BackgroundChecks
WHERE {where}
{orderBy}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("@Offset", pageIndex * pageSize);
        parameters.Add("@PageSize", pageSize);

        totalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);
        var result = await conn.QueryAsync<BackgroundCheck>(dataSql, parameters);
        items = result.ToList();

        return new ArticleSet<BackgroundCheck, int>(items, totalCount);
    }

    public Task<bool> MoveUpAsync(long id) => Task.FromResult(false);
    public Task<bool> MoveDownAsync(long id) => Task.FromResult(false);
}
