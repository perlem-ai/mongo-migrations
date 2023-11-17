namespace Hdp.Infrastructure.Mongo.Migrations.Contracts.Interfaces;

public interface ICollectionNameResolver
{
    string GetName(Type type);
}