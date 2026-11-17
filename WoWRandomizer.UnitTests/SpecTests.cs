using WoWRandomizer.Domain;

namespace WoWRandomizer.UnitTests;

public class SpecTests
{
    [Fact]
    public void Constructor_WithoutTypes_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Spec("Arms", []));
    }

    [Fact]
    public void Is_ReturnsTrueOnlyForTheSpecsTypes()
    {
        var feral = new Spec("Feral Combat", SpecType.Tank, SpecType.DPS);

        Assert.True(feral.Is(SpecType.Tank));
        Assert.True(feral.Is(SpecType.DPS));
        Assert.False(feral.Is(SpecType.Healer));
    }

    [Fact]
    public void Equality_IgnoresTypeOrderAndDuplicates()
    {
        var first = new Spec("Feral Combat", SpecType.Tank, SpecType.DPS);
        var second = new Spec("Feral Combat", SpecType.DPS, SpecType.Tank, SpecType.DPS);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equality_DiffersByNameOrTypes()
    {
        var arms = new Spec("Arms", SpecType.DPS);

        Assert.NotEqual(arms, new Spec("Fury", SpecType.DPS));
        Assert.NotEqual(arms, new Spec("Arms", SpecType.Tank));
    }
}
