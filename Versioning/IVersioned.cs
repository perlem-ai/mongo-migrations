namespace Hdp.Versioning;

public interface IVersioned
{
    DataVersion Version { get; set; }
}