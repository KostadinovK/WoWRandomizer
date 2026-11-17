namespace WoWRandomizer.Domain;

/// <summary>
/// A talent specialization of a class. A spec can be played in more than one role
/// (e.g. Feral Combat is both Tank and DPS). Specs are identified by class plus name,
/// since names repeat across classes; the class lives on <see cref="Combo"/>.
/// </summary>
public sealed record Spec
{
    public Spec(string name, IEnumerable<SpecType> types)
    {
        var distinctTypes = types
            .Distinct()
            .Order()
            .ToArray();

        if (distinctTypes.Length == 0)
        {
            throw new ArgumentException("A spec must have at least one type.", nameof(types));
        }

        Name = name;
        Types = distinctTypes;
    }

    public Spec(string name, params SpecType[] types)
        : this(name, (IEnumerable<SpecType>)types)
    {
    }

    public string Name { get; }

    public IReadOnlyList<SpecType> Types { get; }

    /// <summary>Whether this spec can be played as the given type.</summary>
    public bool Is(SpecType type) => Types.Contains(type);

    public bool Equals(Spec? other) =>
        other is not null && Name == other.Name && Types.SequenceEqual(other.Types);

    public override int GetHashCode() =>
        Types.Aggregate(Name.GetHashCode(), HashCode.Combine);
}
