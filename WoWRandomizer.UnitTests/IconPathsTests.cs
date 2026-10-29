using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;
using WoWRandomizer.Web.Services;

namespace WoWRandomizer.UnitTests;

public class IconPathsTests
{
    private static string RealDataPath => Path.Combine(AppContext.BaseDirectory, "Data", "combos.json");

    // ---------- naming convention ----------

    [Theory]
    [InlineData("Orc", "icons/races/orc.png")]
    [InlineData("Night Elf", "icons/races/night-elf.png")]
    [InlineData("Horde Skyborne", "icons/races/horde-skyborne.png")]
    [InlineData("  Alliance   Skyborne ", "icons/races/alliance-skyborne.png")]
    [InlineData("UNDEAD", "icons/races/undead.png")]
    public void Race_UsesLowerCaseNameWithDashes(string name, string expectedPath)
    {
        Assert.Equal(expectedPath, IconPaths.Race(name));
    }

    [Fact]
    public void Class_And_Faction_UseTheirOwnFolders()
    {
        Assert.Equal("icons/classes/warrior.png", IconPaths.Class("Warrior"));
        Assert.Equal("icons/factions/horde.png", IconPaths.Faction("Horde"));
    }

    // ---------- every name in the data has an icon file ----------

    [Fact]
    public void EveryRaceInTheData_HasAnIconFile()
    {
        var races = JsonComboProvider.FromFile(RealDataPath).GetAll().Select(combo => combo.Race).Distinct().ToList();

        Assert.Equal(10, races.Count);
        Assert.All(races, race => AssertIconFileExists(IconPaths.Race(race)));
    }

    [Fact]
    public void EveryClassInTheData_HasAnIconFile()
    {
        var classes = JsonComboProvider.FromFile(RealDataPath).GetAll().Select(combo => combo.CharacterClass).Distinct().ToList();

        Assert.Equal(9, classes.Count);
        Assert.All(classes, characterClass => AssertIconFileExists(IconPaths.Class(characterClass)));
    }

    [Fact]
    public void EveryFaction_HasAnIconFile()
    {
        var factions = Enum.GetValues<Faction>();

        Assert.Equal(2, factions.Length);
        Assert.All(factions, faction => AssertIconFileExists(IconPaths.Faction(faction.ToString())));
    }

    private static void AssertIconFileExists(string iconPath)
    {
        var file = Path.Combine(FindWebWwwroot(), iconPath);

        Assert.True(File.Exists(file), $"Missing icon file: {file}");
    }

    /// <summary>Walks up from the test output folder to the repository root and returns the web project's wwwroot.</summary>
    private static string FindWebWwwroot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var wwwroot = Path.Combine(directory.FullName, "WoWRandomizer.Web", "wwwroot");

            if (Directory.Exists(wwwroot))
            {
                return wwwroot;
            }
        }

        throw new DirectoryNotFoundException("Could not find WoWRandomizer.Web/wwwroot above the test output folder.");
    }
}
