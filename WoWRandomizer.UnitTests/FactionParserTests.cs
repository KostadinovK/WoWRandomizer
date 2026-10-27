using WoWRandomizer.Domain;

namespace WoWRandomizer.UnitTests;

public class FactionParserTests
{
    [Theory]
    [InlineData("horde", Faction.Horde)]
    [InlineData("Horde", Faction.Horde)]
    [InlineData("HORDE", Faction.Horde)]
    [InlineData(" horde ", Faction.Horde)]
    [InlineData("alliance", Faction.Alliance)]
    [InlineData("Alliance", Faction.Alliance)]
    [InlineData("ALLIANCE", Faction.Alliance)]
    public void TryParse_ValidName_ReturnsTheFaction(string value, Faction expected)
    {
        var parsed = FactionParser.TryParse(value, out var faction);

        Assert.True(parsed);
        Assert.Equal(expected, faction);
    }

    [Theory]
    [InlineData("elves")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("5")]
    [InlineData("horde,alliance")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void TryParse_InvalidValue_ReturnsFalse(string? value)
    {
        var parsed = FactionParser.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void AcceptedValues_ListsEveryFactionInLowerCase()
    {
        Assert.Equal(new[] { "horde", "alliance" }, FactionParser.AcceptedValues);
    }
}
