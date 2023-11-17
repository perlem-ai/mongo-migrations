namespace Hdp.Infrastructure.Mongo.Migrations.Contracts.Attributes;

/// <summary>
/// Indicates transient nature of the migration, which needs to be removed after a certain date.
/// The only supported argument format is "short date" (d): "20/03/2023"
/// </summary>
public class TransientMigrationAttribute : Attribute
{
    public DateOnly ExpirationDate { get; }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="expirationDate">Expiration date in "short date" (d) format: "20/03/2023"</param>
    /// <exception cref="ArgumentException">Expiration date parsing error</exception>
    public TransientMigrationAttribute(string expirationDate)
    {
        if (!DateOnly.TryParseExact(expirationDate, "dd/MM/yyyy", out var parsed))
        {
            throw new ArgumentException("Could not parse date from attribute arguments. " +
                                        "Please specify a valid string, e.g.\"20/03/2023\"", nameof(expirationDate));
        }
        
        ExpirationDate = parsed;
    }
}