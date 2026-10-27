using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;

namespace WoWRandomizer.UnitTests;

public class ComboServiceTests
{
    private static readonly Combo[] SampleCombos =
    [
        new("Orc", "Warrior", Faction.Horde),
        new("Human", "Mage", Faction.Alliance),
        new("Troll", "Priest", Faction.Horde)
    ];

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
        var provider = JsonComboProvider.FromFile(Path.Combine(AppContext.BaseDirectory, "Data", "combos.json"));
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

    private sealed class FakeComboProvider : IComboProvider
    {
        private readonly IReadOnlyList<Combo> _combos;

        public FakeComboProvider(params Combo[] combos)
        {
            _combos = combos;
        }

        public IReadOnlyList<Combo> GetAll() => _combos;
    }

    /// <summary>Always picks the last valid index and remembers the upper bound it was asked for.</summary>
    private sealed class LastIndexRandom : Random
    {
        public int RequestedUpperBound { get; private set; }

        public override int Next(int maxValue)
        {
            RequestedUpperBound = maxValue;
            return maxValue - 1;
        }
    }
}
