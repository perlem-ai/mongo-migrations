using Hdp.Infrastructure.Mongo.Migrations.Contracts.Models;

namespace Hdp.Infrastructure.Mongo.Migrations.Contracts.Interfaces;

public interface IDocumentSettingsProvider
{
    DocumentSettings? Get<TDocument>();
}