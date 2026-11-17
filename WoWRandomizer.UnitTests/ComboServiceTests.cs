using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;

namespace WoWRandomizer.UnitTests;

public class ComboServiceTests
{
    private static readonly Combo[] SampleCombos =
    [
        new("Orc", "Warrior", Faction.Horde, new Spec("Arms", SpecType.DPS)),
        new("Human", "Mage", Faction.Alliance, new Spec("Fire", SpecType.DPS)),
        new("Troll", "Priest", Faction.Horde, new Spec("Holy", SpecType.Healer))
    ];

    private static JsonComboProvider RealProvider =>
        JsonComboProvider.FromFile(Path.Combine(AppContext.BaseDirectory, "Data", "combos.json"));

    // ---------- no faction (US-1) ----------

    [Fact]
    public void GetRandom_ReturnsAComboFromTheProvider()
    {
        var service = new ComboService(new FakeComboProvider(SampleCombos), new Random(42));

        var results = Enumerable.Range(0, 200)
            .Select(_ => service.GetRandom())
            .ToList();

        Assert.All(results, combo => Assert.Contains(combo, SampleCombos));
    }

    [Fact]
    public void GetRandom_PicksByIndexAcrossTheWholeList()
    {
        var random = new LastIndexRandom();
        var service = new ComboService(new FakeComboProvider(SampleCombos), random);

        var combo = service.GetRandom();

        Assert.Equal(SampleCombos.Length, random.RequestedUpperBound);
        Assert.Equal(SampleCombos[^1], combo);
    }

    [Fact]
    public void GetRandom_CanReturnEveryCombo()
    {
        var service = new ComboService(new FakeComboProvider(SampleCombos), new Random(1));

        var distinctResults = Enumerable.Range(0, 300)
            .Select(_ => service.GetRandom())
            .Distinct()
            .ToList();

        Assert.Equal(SampleCombos.Length, distinctResults.Count);
    }

    [Fact]
    public void GetRandom_WithTheRealData_CanReturnEveryValidCombo()
    {
        var provider = RealProvider;
        var service = new ComboService(provider, new Random(7));

        var distinctResults = Enumerable.Range(0, 5000)
            .Select(_ => service.GetRandom())
            .ToHashSet();

        Assert.Equal(provider.GetAll().Count, distinctResults.Count);
        Assert.All(distinctResults, combo => Assert.Contains(combo, provider.GetAll()));
    }

    [Fact]
    public void GetRandom_WithNoCombos_Throws()
    {
        var service = new ComboService(new FakeComboProvider(), new Random(0));

        Assert.Throws<InvalidOperationException>(() => service.GetRandom());
    }

    // ---------- with faction (US-2) ----------

    [Theory]
    [InlineData(Faction.Horde)]
    [InlineData(Faction.Alliance)]
    public void GetRandom_WithFaction_ReturnsOnlyCombosOfThatFaction(Faction faction)
    {
        var service = new ComboService(new FakeComboProvider(SampleCombos), new Random(42));

        var results = Enumerable.Range(0, 200)
            .Select(_ => service.GetRandom(faction))
            .ToList();

        Assert.All(results, combo => Assert.Equal(faction, combo.Faction));
    }

    [Fact]
    public void GetRandom_WithFaction_PicksByIndexAcrossOnlyThatFactionsCombos()
    {
        var random = new LastIndexRandom();
        var service = new ComboService(new FakeComboProvider(SampleCombos), random);

        var combo = service.GetRandom(Faction.Horde);

        // Two of the three sample combos are Horde, so the pick is made over 2 combos, not 3.
        Assert.Equal(2, random.RequestedUpperBound);
        Assert.Equal(SampleCombos[2], combo);
    }

    [Theory]
    [InlineData(Faction.Horde)]
    [InlineData(Faction.Alliance)]
    public void GetRandom_WithFaction_AndTheRealData_CanReturnEveryComboOfThatFaction(Faction faction)
    {
        var provider = RealProvider;
        var service = new ComboService(provider, new Random(7));

        var expected = provider.GetAll()
            .Where(combo => combo.Faction == faction)
            .ToHashSet();

        var results = Enumerable.Range(0, 3000)
            .Select(_ => service.GetRandom(faction))
            .ToHashSet();

        Assert.Equal(84, expected.Count);
        Assert.True(expected.SetEquals(results));
    }

    [Fact]
    public void GetRandom_WithFactionThatHasNoCombos_Throws()
    {
        var hordeOnly = new FakeComboProvider(SampleCombos[0]);
        var service = new ComboService(hordeOnly, new Random(0));

        Assert.Throws<InvalidOperationException>(() => service.GetRandom(Faction.Alliance));
    }
}
