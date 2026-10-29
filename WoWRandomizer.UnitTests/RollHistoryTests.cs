using WoWRandomizer.Shared;
using WoWRandomizer.Web.Services;

namespace WoWRandomizer.UnitTests;

public class RollHistoryTests
{
    private readonly IRollHistory _history = new RollHistory();

    private static ComboResponse Combo(int number) => new($"Race{number}", $"Class{number}", "Horde");

    [Fact]
    public void NewHistory_IsEmpty()
    {
        Assert.Empty(_history.Entries);
    }

    [Fact]
    public void Add_PutsTheNewestRollFirst()
    {
        _history.Add(Combo(1));
        _history.Add(Combo(2));
        _history.Add(Combo(3));

        Assert.Equal([Combo(3), Combo(2), Combo(1)], _history.Entries);
    }

    [Fact]
    public void Add_KeepsExactlyTenRolls()
    {
        Enumerable.Range(1, RollHistory.MaxEntries).ToList().ForEach(number => _history.Add(Combo(number)));

        Assert.Equal(10, _history.Entries.Count);
        Assert.Equal(Combo(1), _history.Entries[^1]);
    }

    [Fact]
    public void Add_WhenFull_DropsTheOldestRollAndKeepsTheOrder()
    {
        Enumerable.Range(1, 12).ToList().ForEach(number => _history.Add(Combo(number)));

        Assert.Equal(Enumerable.Range(3, 10).Reverse().Select(Combo), _history.Entries);
    }

    [Fact]
    public void Add_TheSameComboTwice_KeepsBothEntries()
    {
        _history.Add(Combo(1));
        _history.Add(Combo(1));

        Assert.Equal(2, _history.Entries.Count);
    }

    [Fact]
    public void Clear_RemovesEveryEntry()
    {
        _history.Add(Combo(1));
        _history.Add(Combo(2));

        _history.Clear();

        Assert.Empty(_history.Entries);
    }

    [Fact]
    public void Clear_OnAnEmptyHistory_DoesNothing()
    {
        _history.Clear();

        Assert.Empty(_history.Entries);
    }

    [Fact]
    public void Add_AfterClear_StartsANewHistory()
    {
        _history.Add(Combo(1));
        _history.Clear();

        _history.Add(Combo(2));

        Assert.Equal([Combo(2)], _history.Entries);
    }

    [Fact]
    public void Entries_ReturnedEarlier_AreNotChangedByLaterRolls()
    {
        _history.Add(Combo(1));
        var snapshot = _history.Entries;

        _history.Add(Combo(2));

        Assert.Equal([Combo(1)], snapshot);
    }

    [Fact]
    public void Add_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _history.Add(null!));
    }
}
