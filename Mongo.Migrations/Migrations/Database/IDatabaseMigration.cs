using Hdp.Versioning;

namespace Hdp.Infrastructure.Mongo.Migrations.Migrations.Database;

public interface IDatabaseMigration
{
    DataVersion GetVersion();
    Task Up(CancellationToken cancellationToken);
    Task Down(CancellationToken cancellationToken);
}