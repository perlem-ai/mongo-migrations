namespace Hdp.Infrastructure.Mongo.Migrations.Contracts.Models;

public abstract record OptimisticConcurrencyStrategy(string BsonPropertyName, Func<object> StampFactory);

public sealed record UniqueIdStamp(string BsonPropertyName) 
    : OptimisticConcurrencyStrategy(BsonPropertyName, () => Guid.NewGuid());
    