using Azunt.Models.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Azunt.BackgroundCheckManagement;

public class BackgroundCheckRepositoryAdoNet : IBackgroundCheckRepository
{
    private readonly string _connectionString;
    private readonly ILogger<BackgroundCheckRepositoryAdoNet> _logger;

    public BackgroundCheckRepositoryAdoNet(string connectionString, ILoggerFactory loggerFactory)
    {
        _connectionString = connectionString;
        _logger = loggerFactory.CreateLogger<BackgroundCheckRepositoryAdoNet>();
    }

    private SqlConnection GetConnection() => new(_connectionString);

    public async Task<BackgroundCheck> AddAsync(BackgroundCheck model)
    {
        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO BackgroundChecks (
    Active, BackgroundCheckId, BackgroundStatus, CompletedAt,
    CreatedAt, CreatedBy, EmployeeId, FileName, InvestigationId,
    PackageId, BillCodeId, Provider, ReportUrl, Score, Status,
    UpdatedAt, VendorId
)
OUTPUT INSERTED.Id
VALUES (
    @Active, @BackgroundCheckId, @BackgroundStatus, @CompletedAt,
    @CreatedAt, @CreatedBy, @EmployeeId, @FileName, @InvestigationId,
    @PackageId, @BillCodeId, @Provider, @ReportUrl, @Score, @Status,
    @UpdatedAt, @VendorId
)";
        cmd.Parameters.AddWithValue("@Active", model.Active ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@BackgroundCheckId", model.BackgroundCheckId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@BackgroundStatus", model.BackgroundStatus ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CompletedAt", model.CompletedAt ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedAt", model.CreatedAt ?? DateTimeOffset.UtcNow);
        cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@EmployeeId", model.EmployeeId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FileName", model.FileName ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@InvestigationId", model.InvestigationId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@PackageId", model.PackageId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@BillCodeId", model.BillCodeId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Provider", model.Provider ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ReportUrl", model.ReportUrl ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Score", model.Score ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Status", model.Status ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@UpdatedAt", model.UpdatedAt ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@VendorId", model.VendorId ?? (object)DBNull.Value);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        model.Id = (long)result!;
        return model;
    }

    public async Task<IEnumerable<BackgroundCheck>> GetAllAsync()
    {
        var list = new List<BackgroundCheck>();
        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM BackgroundChecks ORDER BY Id DESC";

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadBackgroundCheck(reader));
        }

        return list;
    }

    public async Task<BackgroundCheck> GetByIdAsync(long id)
    {
        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM BackgroundChecks WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadBackgroundCheck(reader);
        }

        return new BackgroundCheck();
    }

    public async Task<bool> UpdateAsync(BackgroundCheck model)
    {
        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
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

        cmd.Parameters.AddWithValue("@Id", model.Id);
        cmd.Parameters.AddWithValue("@Active", model.Active ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@BackgroundCheckId", model.BackgroundCheckId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@BackgroundStatus", model.BackgroundStatus ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CompletedAt", model.CompletedAt ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@EmployeeId", model.EmployeeId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FileName", model.FileName ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@InvestigationId", model.InvestigationId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@PackageId", model.PackageId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@BillCodeId", model.BillCodeId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Provider", model.Provider ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ReportUrl", model.ReportUrl ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Score", model.Score ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Status", model.Status ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@UpdatedAt", model.UpdatedAt ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@VendorId", model.VendorId ?? (object)DBNull.Value);

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM BackgroundChecks WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<ArticleSet<BackgroundCheck, int>> GetAllAsync<TParentIdentifier>(int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier, string category = "")
    {
        var items = new List<BackgroundCheck>();
        int totalCount = 0;

        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();

        var where = "1=1";
        if (!string.IsNullOrWhiteSpace(searchQuery) && searchField == "Provider")
        {
            where += " AND Provider LIKE @SearchQuery";
            cmd.Parameters.AddWithValue("@SearchQuery", $"%{searchQuery}%");
        }

        var orderBy = sortOrder switch
        {
            "CreatedAt" => "ORDER BY CreatedAt DESC",
            "Provider" => "ORDER BY Provider",
            _ => "ORDER BY Id DESC"
        };

        cmd.CommandText = $@"
SELECT COUNT(*) FROM BackgroundChecks WHERE {where};
SELECT * FROM BackgroundChecks WHERE {where} {orderBy} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        cmd.Parameters.AddWithValue("@Offset", pageIndex * pageSize);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            items.Add(ReadBackgroundCheck(reader));
        }

        return new ArticleSet<BackgroundCheck, int>(items, totalCount);
    }

    public Task<bool> MoveUpAsync(long id) => Task.FromResult(false);
    public Task<bool> MoveDownAsync(long id) => Task.FromResult(false);

    private static BackgroundCheck ReadBackgroundCheck(SqlDataReader reader) => new()
    {
        Id = reader.GetInt64(reader.GetOrdinal("Id")),
        Active = reader.IsDBNull(reader.GetOrdinal("Active")) ? null : reader.GetBoolean(reader.GetOrdinal("Active")),
        BackgroundCheckId = reader["BackgroundCheckId"] as string,
        BackgroundStatus = reader["BackgroundStatus"] as string,
        CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt")) ? null : reader.GetDateTimeOffset(reader.GetOrdinal("CompletedAt")),
        CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTimeOffset(reader.GetOrdinal("CreatedAt")),
        CreatedBy = reader["CreatedBy"] as string,
        EmployeeId = reader.IsDBNull(reader.GetOrdinal("EmployeeId")) ? null : reader.GetInt64(reader.GetOrdinal("EmployeeId")),
        FileName = reader["FileName"] as string,
        InvestigationId = reader.IsDBNull(reader.GetOrdinal("InvestigationId")) ? null : reader.GetInt64(reader.GetOrdinal("InvestigationId")),
        PackageId = reader["PackageId"] as string,
        BillCodeId = reader["BillCodeId"] as string,
        Provider = reader["Provider"] as string,
        ReportUrl = reader["ReportUrl"] as string,
        Score = reader["Score"] as string,
        Status = reader["Status"] as string,
        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTimeOffset(reader.GetOrdinal("UpdatedAt")),
        VendorId = reader.IsDBNull(reader.GetOrdinal("VendorId")) ? null : reader.GetInt64(reader.GetOrdinal("VendorId"))
    };
}