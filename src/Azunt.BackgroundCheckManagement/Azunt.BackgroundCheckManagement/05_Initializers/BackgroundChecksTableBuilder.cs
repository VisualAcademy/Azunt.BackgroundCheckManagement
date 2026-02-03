using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.BackgroundCheckManagement
{
    /// <summary>
    /// 각 테넌트 DB(및 마스터 DB)에 BackgroundChecks 테이블을
    /// "존재하면 유지 + 누락 컬럼만 추가" 방식으로 안전하게 정규화합니다.
    /// - FK/외부 제약은 생성하지 않습니다.
    /// - 새 컬럼은 NULL 허용, 가능한 곳은 기본값(DEFAULT) 포함.
    /// - 인덱스는 존재하지 않을 때만 생성(선택적 성능 개선).
    /// - 재실행에 안전(idempotent).
    /// </summary>
    public class BackgroundChecksTableBuilder
    {
        private readonly string _connectionString;
        private readonly ILogger<BackgroundChecksTableBuilder> _logger;

        public BackgroundChecksTableBuilder(string connectionString, ILogger<BackgroundChecksTableBuilder> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public void BuildMasterDatabase()
        {
            try
            {
                EnsureBackgroundChecksTable(_connectionString);
                _logger.LogInformation("BackgroundChecks table processed (master DB).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BackgroundChecks table (master DB).");
            }
        }

        public void BuildTenantDatabases()
        {
            var tenantConnectionStrings = GetTenantConnectionStrings();

            for (int i = 0; i < tenantConnectionStrings.Count; i++)
            {
                var connStr = tenantConnectionStrings[i];
                var tenantIndex = i + 1; // 1-based index for logs

                try
                {
                    EnsureBackgroundChecksTable(connStr);
                    _logger.LogInformation("BackgroundChecks table processed (tenant DB #{Index}).", tenantIndex);
                }
                catch (Exception ex)
                {
                    // 커넥션 스트링/서버/DB 이름 등 민감 정보 미노출
                    _logger.LogError(ex, "Error processing tenant DB #{Index}.", tenantIndex);
                }
            }
        }

        private List<string> GetTenantConnectionStrings()
        {
            var result = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var cmd = new SqlCommand("SELECT ConnectionString FROM dbo.Tenants", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var connStr = reader["ConnectionString"]?.ToString();
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    result.Add(connStr);
                }
            }

            return result;
        }

        private void EnsureBackgroundChecksTable(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            // 1) 테이블 존재 확인 (스키마 고정)
            using var checkTableCmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BackgroundChecks';", connection);

            int exists = (int)checkTableCmd.ExecuteScalar();

            if (exists == 0)
            {
                // 2) 테이블 신규 생성 (주석 포함 / 카테고리 정렬 / CONSTRAINT 키워드 미사용)
                using var createCmd = new SqlCommand(@"
-- [0][0] 백그라운드체크: BackgroundChecks
CREATE TABLE [dbo].[BackgroundChecks] (
    -- 기본 키
    [ID]                BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,

    -- 활성 여부
    [Active]            BIT NULL DEFAULT (1),

    -- 외부 식별자 / 코드
    [BackgroundCheckID] NVARCHAR(MAX) NULL,
    [InvestigationID]   BIGINT NULL,
    [PackageID]         NVARCHAR(MAX) NULL,
    [BillCodeID]        NVARCHAR(MAX) NULL,

    -- 상태 / 점수
    [BackgroundStatus]  NVARCHAR(MAX) NULL,
    [Status]            NVARCHAR(MAX) NULL,
    [Score]             NVARCHAR(MAX) NULL,

    -- 파일 / 리포트 정보
    [FileName]          NVARCHAR(MAX) NULL,
    [ReportURL]         NVARCHAR(MAX) NULL,
    [Provider]          NVARCHAR(MAX) NULL,

    -- 관계 키
    [EmployeeID]        BIGINT NULL,
    [VendorID]          BIGINT NULL,

    -- 감사(로그) 정보
    [CreatedAt]         DATETIMEOFFSET(7) NULL,
    [CreatedBy]         NVARCHAR(70) NULL,
    [CompletedAt]       DATETIMEOFFSET(7) NULL,
    [UpdatedAt]         DATETIMEOFFSET(7) NULL
);", connection);

                createCmd.ExecuteNonQuery();
                _logger.LogInformation("BackgroundChecks table created.");

                // 인덱스(선택): 존재하지 않을 때만 생성
                EnsureIndex(connection, "IX_BackgroundChecks_EmployeeID", "EmployeeID");
                EnsureIndex(connection, "IX_BackgroundChecks_InvestigationID", "InvestigationID");
                EnsureIndex(connection, "IX_BackgroundChecks_VendorID", "VendorID");
            }
            else
            {
                // 3) 기존 테이블: 누락 컬럼만 추가 (NULL/DEFAULT 포함)
                //    ※ 컬럼명 변경 금지 / 타입은 기존과 동일하게 유지(NVARCHAR(MAX) 등)
                var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Active",            "BIT NULL" }, // DEFAULT는 별도 보강
                    { "BackgroundCheckID", "NVARCHAR(MAX) NULL" },
                    { "InvestigationID",   "BIGINT NULL" },
                    { "PackageID",         "NVARCHAR(MAX) NULL" },
                    { "BillCodeID",        "NVARCHAR(MAX) NULL" },

                    { "BackgroundStatus",  "NVARCHAR(MAX) NULL" },
                    { "Status",            "NVARCHAR(MAX) NULL" },
                    { "Score",             "NVARCHAR(MAX) NULL" },

                    { "FileName",          "NVARCHAR(MAX) NULL" },
                    { "ReportURL",         "NVARCHAR(MAX) NULL" },
                    { "Provider",          "NVARCHAR(MAX) NULL" },

                    { "EmployeeID",        "BIGINT NULL" },
                    { "VendorID",          "BIGINT NULL" },

                    { "CreatedAt",         "DATETIMEOFFSET(7) NULL" },
                    { "CreatedBy",         "NVARCHAR(70) NULL" },
                    { "CompletedAt",       "DATETIMEOFFSET(7) NULL" },
                    { "UpdatedAt",         "DATETIMEOFFSET(7) NULL" }
                };

                foreach (var column in columns)
                {
                    using var checkColumnCmd = new SqlCommand(@"
                        SELECT COUNT(*)
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = 'dbo'
                          AND TABLE_NAME = 'BackgroundChecks'
                          AND COLUMN_NAME = @ColumnName;", connection);
                    checkColumnCmd.Parameters.AddWithValue("@ColumnName", column.Key);

                    int columnExists = (int)checkColumnCmd.ExecuteScalar();
                    if (columnExists == 0)
                    {
                        using var alterCmd = new SqlCommand($@"
ALTER TABLE [dbo].[BackgroundChecks]
ADD [{column.Key}] {column.Value};", connection);

                        alterCmd.ExecuteNonQuery();
                        _logger.LogInformation("Column [{Col}] added to BackgroundChecks table.", column.Key);
                    }
                }

                // PK 보강: ID에 PK가 없으면 추가 (CONSTRAINT 키워드 없이)
                EnsurePrimaryKeyOnId(connection);

                // Active 컬럼의 DEFAULT(1) 없으면 보강 (CONSTRAINT 키워드 없이)
                EnsureActiveDefault(connection);

                // 인덱스(선택): 존재하지 않을 때만 생성
                EnsureIndex(connection, "IX_BackgroundChecks_EmployeeID", "EmployeeID");
                EnsureIndex(connection, "IX_BackgroundChecks_InvestigationID", "InvestigationID");
                EnsureIndex(connection, "IX_BackgroundChecks_VendorID", "VendorID");
            }
        }

        /// <summary>
        /// [ID] 컬럼에 Primary Key가 없으면 추가합니다.
        /// - CONSTRAINT 키워드 없이 추가합니다.
        /// - 재실행 안전.
        /// </summary>
        private void EnsurePrimaryKeyOnId(SqlConnection connection)
        {
            using var cmd = new SqlCommand(@"
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BackgroundChecks' AND COLUMN_NAME = 'ID'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.key_constraints kc
        WHERE kc.parent_object_id = OBJECT_ID(N'[dbo].[BackgroundChecks]')
          AND kc.type = 'PK'
    )
    BEGIN
        ALTER TABLE [dbo].[BackgroundChecks]
        ADD PRIMARY KEY CLUSTERED ([ID] ASC);
    END
END
", connection);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Active 컬럼에 DEFAULT (1)이 없으면 추가합니다.
        /// - CONSTRAINT 키워드 없이 "ADD DEFAULT (1) FOR [Active]" 형태로 추가합니다.
        /// - 기존 값은 변경하지 않습니다.
        /// - 재실행 안전.
        /// </summary>
        private void EnsureActiveDefault(SqlConnection connection)
        {
            using var cmd = new SqlCommand(@"
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BackgroundChecks' AND COLUMN_NAME = 'Active'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c
            ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[BackgroundChecks]')
          AND c.name = N'Active'
    )
    BEGIN
        ALTER TABLE [dbo].[BackgroundChecks]
        ADD DEFAULT (1) FOR [Active];
    END
END
", connection);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 인덱스가 없을 때만 생성합니다. (존재하면 아무 것도 하지 않음)
        /// </summary>
        private void EnsureIndex(SqlConnection connection, string indexName, string columnName)
        {
            using var cmd = new SqlCommand(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = @IndexName AND object_id = OBJECT_ID(N'[dbo].[BackgroundChecks]')
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) =
        N'CREATE NONCLUSTERED INDEX [' + @IndexName + N'] ON [dbo].[BackgroundChecks]([' + @ColumnName + N'] ASC);';
    EXEC sp_executesql @sql;
END
", connection);

            cmd.Parameters.AddWithValue("@IndexName", indexName);
            cmd.Parameters.AddWithValue("@ColumnName", columnName);
            cmd.ExecuteNonQuery();
        }

        public static void Run(IServiceProvider services, bool forMaster, string? optionalConnectionString = null)
        {
            try
            {
                var logger = services.GetRequiredService<ILogger<BackgroundChecksTableBuilder>>();
                var config = services.GetRequiredService<IConfiguration>();

                string connectionString;
                if (!string.IsNullOrWhiteSpace(optionalConnectionString))
                {
                    connectionString = optionalConnectionString!;
                }
                else
                {
                    var tempConnectionString = config.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(tempConnectionString))
                    {
                        throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");
                    }
                    connectionString = tempConnectionString;
                }

                var builder = new BackgroundChecksTableBuilder(connectionString, logger);

                if (forMaster)
                {
                    builder.BuildMasterDatabase();
                }
                else
                {
                    builder.BuildTenantDatabases();
                }
            }
            catch (Exception ex)
            {
                // services.GetService: 실패해도 터지지 않도록
                var fallbackLogger = services.GetService<ILogger<BackgroundChecksTableBuilder>>();
                fallbackLogger?.LogError(ex, "Error running BackgroundChecksTableBuilder.Run");
            }
        }
    }
}
