using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.BackgroundCheckManagement
{
    public class BackgroundChecksTableBuilder
    {
        private readonly string _connectionString;
        private readonly ILogger<BackgroundChecksTableBuilder> _logger;

        public BackgroundChecksTableBuilder(string connectionString, ILogger<BackgroundChecksTableBuilder> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public void Build()
        {
            try
            {
                EnsureBackgroundChecksTable(_connectionString);
                _logger.LogInformation("BackgroundChecks table ensured successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring BackgroundChecks table.");
            }
        }

        private void EnsureBackgroundChecksTable(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            // Check if table exists
            var checkTableCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'BackgroundChecks' AND TABLE_SCHEMA = 'dbo'", connection);

            int exists = (int)checkTableCmd.ExecuteScalar();

            if (exists == 0)
            {
                var createCmd = new SqlCommand(@"
CREATE TABLE [dbo].[BackgroundChecks] (
    [ID]                BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Active]            BIT DEFAULT ((1)) NULL,
    [BackgroundCheckID] NVARCHAR(MAX) NULL,
    [BackgroundStatus]  NVARCHAR(MAX) NULL,
    [CompletedAt]       DATETIMEOFFSET(7) NULL,
    [CreatedAt]         DATETIMEOFFSET(7) NULL,
    [CreatedBy]         NVARCHAR(70) NULL,
    [EmployeeID]        BIGINT NULL,
    [FileName]          NVARCHAR(MAX) NULL,
    [InvestigationID]   BIGINT NULL,
    [PackageID]         NVARCHAR(MAX) NULL,
    [Provider]          NVARCHAR(MAX) NULL,
    [ReportURL]         NVARCHAR(MAX) NULL,
    [Score]             NVARCHAR(MAX) NULL,
    [Status]            NVARCHAR(MAX) NULL,
    [UpdatedAt]         DATETIMEOFFSET(7) NULL,
    [VendorID]          BIGINT NULL
)", connection);

                createCmd.ExecuteNonQuery();
                _logger.LogInformation("BackgroundChecks table created.");
            }
            else
            {
                var columns = new Dictionary<string, string>
                {
                    { "Active", "BIT DEFAULT ((1)) NULL" },
                    { "BackgroundCheckID", "NVARCHAR(MAX) NULL" },
                    { "BackgroundStatus", "NVARCHAR(MAX) NULL" },
                    { "CompletedAt", "DATETIMEOFFSET(7) NULL" },
                    { "CreatedAt", "DATETIMEOFFSET(7) NULL" },
                    { "CreatedBy", "NVARCHAR(70) NULL" },
                    { "EmployeeID", "BIGINT NULL" },
                    { "FileName", "NVARCHAR(MAX) NULL" },
                    { "InvestigationID", "BIGINT NULL" },
                    { "PackageID", "NVARCHAR(MAX) NULL" },
                    { "Provider", "NVARCHAR(MAX) NULL" },
                    { "ReportURL", "NVARCHAR(MAX) NULL" },
                    { "Score", "NVARCHAR(MAX) NULL" },
                    { "Status", "NVARCHAR(MAX) NULL" },
                    { "UpdatedAt", "DATETIMEOFFSET(7) NULL" },
                    { "VendorID", "BIGINT NULL" }
                };

                foreach (var column in columns)
                {
                    var checkColumnCmd = new SqlCommand(@"
                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'BackgroundChecks' AND COLUMN_NAME = @ColumnName", connection);
                    checkColumnCmd.Parameters.AddWithValue("@ColumnName", column.Key);

                    int columnExists = (int)checkColumnCmd.ExecuteScalar();
                    if (columnExists == 0)
                    {
                        var alterCmd = new SqlCommand($@"
                            ALTER TABLE [dbo].[BackgroundChecks]
                            ADD [{column.Key}] {column.Value}", connection);
                        alterCmd.ExecuteNonQuery();
                        _logger.LogInformation($"Column [{column.Key}] added to BackgroundChecks table.");
                    }
                }
            }
        }

        public static void Run(IServiceProvider services, string? optionalConnectionString = null)
        {
            try
            {
                var logger = services.GetRequiredService<ILogger<BackgroundChecksTableBuilder>>();
                var config = services.GetRequiredService<IConfiguration>();

                string connectionString = optionalConnectionString
                    ?? config.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string is missing.");

                var builder = new BackgroundChecksTableBuilder(connectionString, logger);
                builder.Build();
            }
            catch (Exception ex)
            {
                var fallbackLogger = services.GetService<ILogger<BackgroundChecksTableBuilder>>();
                fallbackLogger?.LogError(ex, "Error running BackgroundChecksTableBuilder.Run");
            }
        }
    }
}