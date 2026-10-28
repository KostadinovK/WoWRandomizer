namespace WoWRandomizer.Shared;

/// <summary>A random valid WoW Forever character: a race, a class, and the faction of that race.</summary>
/// <param name="Race">The character race, for example Orc.</param>
/// <param name="Class">The character class, for example Warrior.</param>
/// <param name="Faction">The faction of the race: Horde or Alliance.</param>
/// <example>{"race":"Orc","class":"Warrior","faction":"Horde"}</example>
public sealed record ComboResponse(string Race, string Class, string Faction);
