namespace WoWRandomizer.Shared;

/// <summary>A random valid WoW Forever character: a race, a class, the faction of that race, and a spec.</summary>
/// <param name="Race">The character race, for example Orc.</param>
/// <param name="Class">The character class, for example Warrior.</param>
/// <param name="Faction">The faction of the race: Horde or Alliance.</param>
/// <param name="Spec">The spec of the class, with the type(s) it can be played as (Tank, Healer, DPS).</param>
/// <example>{"race":"Orc","class":"Warrior","faction":"Horde","spec":{"name":"Fury","types":["DPS"]}}</example>
public sealed record ComboResponse(string Race, string Class, string Faction, SpecResponse Spec);
