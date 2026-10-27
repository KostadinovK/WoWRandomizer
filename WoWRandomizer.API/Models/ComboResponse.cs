using WoWRandomizer.Domain;

namespace WoWRandomizer.API.Models;

/// <summary>Response body of the combo endpoint, e.g. { "race": "Orc", "class": "Warrior", "faction": "Horde" }.</summary>
public sealed record ComboResponse(string Race, string Class, string Faction)
{
    public static ComboResponse From(Combo combo) =>
        new(combo.Race, combo.CharacterClass, combo.Faction.ToString());
}
