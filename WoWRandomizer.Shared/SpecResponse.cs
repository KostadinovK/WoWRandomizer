namespace WoWRandomizer.Shared;

/// <summary>A talent specialization of a class and the roles it can be played in.</summary>
/// <param name="Name">The spec name, for example Fury.</param>
/// <param name="Types">
/// The roles the spec can be played in, as text: <c>Tank</c>, <c>Healer</c> or <c>DPS</c>.
/// Most specs have one type; a few (for example Feral Combat) have several.
/// </param>
/// <example>{"name":"Fury","types":["DPS"]}</example>
public sealed record SpecResponse(string Name, IReadOnlyList<string> Types)
{
    public bool Equals(SpecResponse? other) =>
        other is not null && Name == other.Name && Types.SequenceEqual(other.Types);

    public override int GetHashCode() =>
        Types.Aggregate(Name.GetHashCode(), (hash, type) => HashCode.Combine(hash, type));
}
