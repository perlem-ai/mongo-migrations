namespace Hdp.Infrastructure.Mongo.Migrations.Contracts.Models;

public record DocumentSettings(string BsonIdPropertyName, OptimisticConcurrencyStrategy? OptimisticConcurrencyStrategy = null)
{
    private const string DefaultBsonIdPropertyName = "_id";
    
    public static DocumentSettings Default 
        => new (DefaultBsonIdPropertyName);
}