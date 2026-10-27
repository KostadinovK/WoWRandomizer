namespace WoWRandomizer.Domain;

/// <summary>Picks random combos from the valid race + class combinations.</summary>
public sealed class ComboService
{
    private readonly IComboProvider _comboProvider;
    private readonly Random _random;

    public ComboService(IComboProvider comboProvider, Random random)
    {
        _comboProvider = comboProvider;
        _random = random;
    }

    /// <summary>Returns a combo picked uniformly at random from all valid combos.</summary>
    public Combo GetRandom()
    {
        var combos = _comboProvider.GetAll();

        if (combos.Count == 0)
        {
            throw new InvalidOperationException("There are no valid combos to choose from.");
        }

        return combos[_random.Next(combos.Count)];
    }
}
