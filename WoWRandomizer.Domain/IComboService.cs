namespace WoWRandomizer.Domain;

/// <summary>Business logic for choosing race + class combos.</summary>
public interface IComboService
{
    /// <summary>Returns a combo picked uniformly at random from all valid combos.</summary>
    Combo GetRandom();
}
