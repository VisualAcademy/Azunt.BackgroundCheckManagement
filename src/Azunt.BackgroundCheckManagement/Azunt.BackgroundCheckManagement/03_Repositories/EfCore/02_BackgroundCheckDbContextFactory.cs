using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Azunt.BackgroundCheckManagement;

public class BackgroundCheckDbContextFactory
{
    private readonly IConfiguration? _configuration;

    public BackgroundCheckDbContextFactory() { }

    public BackgroundCheckDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public BackgroundCheckDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<BackgroundCheckDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BackgroundCheckDbContext(options);
    }

    public BackgroundCheckDbContext CreateDbContext(DbContextOptions<BackgroundCheckDbContext> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new BackgroundCheckDbContext(options);
    }

    public BackgroundCheckDbContext CreateDbContext()
    {
        if (_configuration == null)
        {
            throw new InvalidOperationException("Configuration is not provided.");
        }

        var defaultConnection = _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            throw new InvalidOperationException("DefaultConnection is not configured properly.");
        }

        return CreateDbContext(defaultConnection);
    }
}