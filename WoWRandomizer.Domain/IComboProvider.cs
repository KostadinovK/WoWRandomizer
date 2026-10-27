namespace WoWRandomizer.Domain;

/// <summary>Source of every valid race + class combination in WoW Forever.</summary>
public interface IComboProvider
{
    IReadOnlyList<Combo> GetAll();
}
