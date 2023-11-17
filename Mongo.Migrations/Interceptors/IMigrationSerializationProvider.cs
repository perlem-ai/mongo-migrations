using MongoDB.Bson.Serialization;

namespace Hdp.Infrastructure.Mongo.Migrations.Interceptors;

public interface IMigrationSerializationProvider : IBsonSerializationProvider
{
}