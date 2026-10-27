namespace WoWRandomizer.Domain;

/// <summary>A valid race + class combination, together with the faction of the race.</summary>
public sealed record Combo(string Race, string CharacterClass, Faction Faction);
