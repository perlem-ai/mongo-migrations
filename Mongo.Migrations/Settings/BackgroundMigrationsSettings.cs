namespace Hdp.Infrastructure.Mongo.Migrations.Settings;

public class BackgroundMigrationsSettings
{
    public ServiceShutdownMode ShutdownMode { get; set; } = ServiceShutdownMode.ShutdownWhenMigrationCompleted;
}

public enum ServiceShutdownMode
{
    ShutdownWhenMigrationCompleted = 0
}