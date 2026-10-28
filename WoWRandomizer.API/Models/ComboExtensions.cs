using WoWRandomizer.Domain;
using WoWRandomizer.Shared;

namespace WoWRandomizer.API.Models;

/// <summary>Explicit mapping from the domain <see cref="Combo"/> to the API contract.</summary>
public static class ComboExtensions
{
    public static ComboResponse ToResponse(this Combo combo) =>
        new(combo.Race, combo.CharacterClass, combo.Faction.ToString());
}
