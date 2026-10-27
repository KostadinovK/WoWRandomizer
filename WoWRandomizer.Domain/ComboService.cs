namespace WoWRandomizer.Domain;

/// <summary>Picks random combos from the valid race + class combinations.</summary>
public sealed class ComboService : IComboService
{
    private readonly IComboProvider _comboProvider;
    private readonly Random _random;

    public ComboService(IComboProvider comboProvider, Random random)
    {
        _comboProvider = comboProvider;
        _random = random;
    }

    /// <inheritdoc />
    public Combo GetRandom() =>
        PickRandom(_comboProvider.GetAll(), "There are no valid combos to choose from.");

    /// <inheritdoc />
    public Combo GetRandom(Faction faction)
    {
        var combos = _comboProvider.GetAll()
            .Where(combo => combo.Faction == faction)
            .ToList();

        return PickRandom(combos, $"There are no valid {faction} combos to choose from.");
    }

    private Combo PickRandom(IReadOnlyList<Combo> combos, string noCombosMessage)
    {
        if (combos.Count == 0)
        {
            throw new InvalidOperationException(noCombosMessage);
        }

        return combos[_random.Next(combos.Count)];
    }
}
