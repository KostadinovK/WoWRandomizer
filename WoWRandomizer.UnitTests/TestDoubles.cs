using WoWRandomizer.Domain;

namespace WoWRandomizer.UnitTests;

internal sealed class FakeComboProvider : IComboProvider
{
    private readonly IReadOnlyList<Combo> _combos;

    public FakeComboProvider(params Combo[] combos)
    {
        _combos = combos;
    }

    public IReadOnlyList<Combo> GetAll() => _combos;
}

/// <summary>Always picks the last valid index and remembers the upper bound it was asked for.</summary>
internal sealed class LastIndexRandom : Random
{
    public int RequestedUpperBound { get; private set; }

    public override int Next(int maxValue)
    {
        RequestedUpperBound = maxValue;
        return maxValue - 1;
    }
}
