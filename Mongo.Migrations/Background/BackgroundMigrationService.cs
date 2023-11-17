using Hdp.Infrastructure.Mongo.Migrations.Migrators;
using Hdp.Infrastructure.Mongo.Migrations.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hdp.Infrastructure.Mongo.Migrations.Background;

public class BackgroundMigrationService : BackgroundService
{
    private readonly MigrationRunner _migrationRunner;
    private readonly ILogger<BackgroundMigrationService> _logger;
    private readonly IApplicationLifetime _applicationLifetime;
    private readonly BackgroundMigrationsSettings _settings;

    public BackgroundMigrationService(MigrationRunner migrationRunner, IOptions<BackgroundMigrationsSettings> settings,
        ILogger<BackgroundMigrationService> logger, IApplicationLifetime applicationLifetime)
    {
        _migrationRunner = migrationRunner;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _settings = settings.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _migrationRunner.Migrate(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cancellation requested, shutting down");
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex, $"Background migration job finished with error: {ex.Message}");
            // Adding a delay to allow application finish needed background routines (like sending logs)
            // TODO: Think about a better approach for returning valid error code on application exit
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            Environment.Exit(-1);
        }

        if (_settings.ShutdownMode == ServiceShutdownMode.ShutdownWhenMigrationCompleted)
        {
            // Adding a delay to allow application finish needed initialization routines (like starting built-in hosted services)
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            _logger.LogInformation("Migration job completed successfully, shutting down");
            Environment.Exit(0);
        }
        _logger.LogInformation("Migration job completed successfully, shutting down is disabled");
    }
}