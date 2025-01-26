using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MontrealComputerPerformanceClub.EnterprisePersistentConfiguration;

/// <summary>
/// The moment when a configuration is changed. It represents time in any abstract way, as long as
/// it is (weakly) monotonic, i.e. updates are made in a sequence and the order of the timestamp
/// matches the order of the updates in reality. The expectation is to implement it with a row id in
/// a relational database, which would allow us to tighten the requirements to strictly monotonic,
/// but that would make alternate implementations with an actual timestamp unreliable if the
/// resolution is not good enough to guarantee that any two updates have a different ones!
/// </summary>
public readonly struct ConfigurationTimeStamp(long stamp)
    : IEquatable<ConfigurationTimeStamp>,
        IComparable<ConfigurationTimeStamp>
{
    public static readonly ConfigurationTimeStamp Zero;

    public long Stamp { get; } = stamp;

    public bool Equals(ConfigurationTimeStamp that)
    {
        return Stamp == that.Stamp;
    }

    public override bool Equals([NotNullWhen(true)] object? that)
    {
        return that is ConfigurationTimeStamp { Stamp: var thatStamp } && Stamp == thatStamp;
    }

    public override int GetHashCode()
    {
        return Stamp.GetHashCode();
    }

    public static bool operator ==(ConfigurationTimeStamp left, ConfigurationTimeStamp right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ConfigurationTimeStamp left, ConfigurationTimeStamp right)
    {
        return !left.Equals(right);
    }

    public int CompareTo(ConfigurationTimeStamp that)
    {
        return Stamp.CompareTo(that.Stamp);
    }

    public static bool operator <(ConfigurationTimeStamp left, ConfigurationTimeStamp right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(ConfigurationTimeStamp left, ConfigurationTimeStamp right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(ConfigurationTimeStamp left, ConfigurationTimeStamp right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(ConfigurationTimeStamp left, ConfigurationTimeStamp right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static ConfigurationTimeStamp Max(
        ConfigurationTimeStamp left,
        ConfigurationTimeStamp right
    )
    {
        if (left < right)
        {
            return right;
        }

        return left;
    }

    public override string ToString()
    {
        return Stamp.ToString(CultureInfo.InvariantCulture);
    }
}
