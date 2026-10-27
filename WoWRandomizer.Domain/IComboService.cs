namespace WoWRandomizer.Domain;

/// <summary>Business logic for choosing race + class combos.</summary>
public interface IComboService
{
    /// <summary>Returns a combo picked uniformly at random from all valid combos.</summary>
    Combo GetRandom();

    /// <summary>Returns a combo picked uniformly at random from the valid combos of the given faction.</summary>
    Combo GetRandom(Faction faction);
}
