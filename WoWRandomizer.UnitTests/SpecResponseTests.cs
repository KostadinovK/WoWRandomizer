using WoWRandomizer.Shared;

namespace WoWRandomizer.UnitTests;

public class SpecResponseTests
{
    [Fact]
    public void Equality_ComparesTheTypesByValue()
    {
        var first = new SpecResponse("Feral Combat", ["Tank", "DPS"]);
        var second = new SpecResponse("Feral Combat", new List<string> { "Tank", "DPS" });

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equality_DiffersByNameOrTypes()
    {
        var fury = new SpecResponse("Fury", ["DPS"]);

        Assert.NotEqual(fury, new SpecResponse("Arms", ["DPS"]));
        Assert.NotEqual(fury, new SpecResponse("Fury", ["Tank"]));
    }
}
