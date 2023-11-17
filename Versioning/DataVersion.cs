namespace Hdp.Versioning;

public record DataVersion(int Major, int Minor, int Revision) : IComparable<DataVersion>
{
    public static DataVersion Empty() 
        => new (-1, 0, 0);

    public override string ToString() 
        => $"{Major}.{Minor}.{Revision}";

    public int CompareTo(DataVersion? other)
    {
        if (Equals(other))
            return 0;

        return other != null && this > other ? 1 : -1;
    }

    public static bool operator >(DataVersion a, DataVersion b)
        => a.Major > b.Major
           || (a.Major == b.Major && a.Minor > b.Minor)
           || (a.Major == b.Major && a.Minor == b.Minor && a.Revision > b.Revision);

    public static bool operator <(DataVersion a, DataVersion b) 
        => a != b && !(a > b);

    public static bool operator <=(DataVersion a, DataVersion b) 
        => a == b || a < b;

    public static bool operator >=(DataVersion a, DataVersion b) 
        => a == b || a > b;

    public override int GetHashCode()
    {
        unchecked
        {
            int result = Major;
            result = (result * 397) ^ Minor;
            result = (result * 397) ^ Revision;
            return result;
        }
    }
}