using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;

namespace WoWRandomizer.UnitTests;

public class JsonComboProviderTests
{
    private static string RealDataPath => Path.Combine(AppContext.BaseDirectory, "Data", "combos.json");

    // ---------- the real data file ----------

    [Fact]
    public void RealDataFile_LoadsAll56Combos_28PerFaction()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        Assert.Equal(56, combos.Count);
        Assert.Equal(28, combos.Count(combo => combo.Faction == Faction.Horde));
        Assert.Equal(28, combos.Count(combo => combo.Faction == Faction.Alliance));
    }

    [Theory]
    [InlineData("Orc", 6)]
    [InlineData("Tauren", 4)]
    [InlineData("Troll", 7)]
    [InlineData("Undead", 6)]
    [InlineData("Horde Skyborne", 5)]
    [InlineData("Gnome", 5)]
    [InlineData("Human", 7)]
    [InlineData("Night Elf", 5)]
    [InlineData("Dwarf", 6)]
    [InlineData("Alliance Skyborne", 5)]
    public void RealDataFile_EachRaceHasExpectedNumberOfClasses(string race, int expectedClassCount)
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        Assert.Equal(expectedClassCount, combos.Count(combo => combo.Race == race));
    }

    [Fact]
    public void RealDataFile_EachRaceBelongsToExactlyOneFaction()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        var races = combos
            .GroupBy(combo => combo.Race)
            .ToList();

        Assert.Equal(10, races.Count);
        Assert.All(races, group => Assert.Single(group.Select(combo => combo.Faction).Distinct()));
    }

    [Fact]
    public void RealDataFile_HasNoDuplicateCombos()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        Assert.Equal(combos.Count, combos.Distinct().Count());
    }

    // ---------- parsing ----------

    [Fact]
    public void FromJson_FlattensToOneComboPerRaceClassPair()
    {
        const string json = """
            { "races": [
                { "name": "Orc", "faction": "Horde", "classes": ["Warrior", "Mage"] },
                { "name": "Human", "faction": "Alliance", "classes": ["Paladin"] }
            ] }
            """;

        var combos = JsonComboProvider.FromJson(json).GetAll();

        Assert.Equal(3, combos.Count);
        Assert.Contains(new Combo("Orc", "Warrior", Faction.Horde), combos);
        Assert.Contains(new Combo("Orc", "Mage", Faction.Horde), combos);
        Assert.Contains(new Combo("Human", "Paladin", Faction.Alliance), combos);
    }

    [Fact]
    public void FromJson_FactionIsCaseInsensitive()
    {
        const string json = """{ "races": [ { "name": "Orc", "faction": "hORDE", "classes": ["Warrior"] } ] }""";

        var combo = Assert.Single(JsonComboProvider.FromJson(json).GetAll());

        Assert.Equal(Faction.Horde, combo.Faction);
    }

    [Fact]
    public void FromJson_TrimsWhitespaceInNamesAndClasses()
    {
        const string json = """{ "races": [ { "name": " Orc ", "faction": "Horde", "classes": [" Warrior "] } ] }""";

        var combo = Assert.Single(JsonComboProvider.FromJson(json).GetAll());

        Assert.Equal(new Combo("Orc", "Warrior", Faction.Horde), combo);
    }

    // ---------- validation ----------

    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{ "races": [] }""")]
    [InlineData("""{ "races": [ { "faction": "Horde", "classes": ["Warrior"] } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "classes": ["Warrior"] } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "Elves", "classes": ["Warrior"] } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "5", "classes": ["Warrior"] } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "Horde" } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "Horde", "classes": [] } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Warrior", " "] } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Mage", "mage"] } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Mage"] }, { "name": "orc", "faction": "Horde", "classes": ["Rogue"] } ] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Mage"] }, { "name": "Orc", "faction": "Alliance", "classes": ["Mage"] } ] }""")]
    public void FromJson_InvalidData_Throws(string json)
    {
        Assert.Throws<ComboDataException>(() => JsonComboProvider.FromJson(json));
    }

    // ---------- file handling ----------

    [Fact]
    public void FromFile_MissingFile_Throws()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        Assert.Throws<ComboDataException>(() => JsonComboProvider.FromFile(missingPath));
    }

    [Fact]
    public void FromFile_ChangingTheFile_ChangesTheCombos()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            File.WriteAllText(path, """{ "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Warrior"] } ] }""");
            var before = JsonComboProvider.FromFile(path).GetAll();

            File.WriteAllText(path, """{ "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Warrior", "Mage"] } ] }""");
            var after = JsonComboProvider.FromFile(path).GetAll();

            Assert.Single(before);
            Assert.Equal(2, after.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
