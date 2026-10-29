using WoWRandomizer.Shared;

namespace WoWRandomizer.Web.Services;

/// <summary>Keeps the last rolls in memory. It lives as long as the page, so a reload starts with an empty history.</summary>
public sealed class RollHistory : IRollHistory
{
    public const int MaxEntries = 10;

    private readonly List<ComboResponse> _entries = [];

    // A copy, so a list that was handed out earlier does not change under the caller.
    public IReadOnlyList<ComboResponse> Entries => _entries.ToList();

    public void Add(ComboResponse combo)
    {
        ArgumentNullException.ThrowIfNull(combo);

        _entries.Insert(0, combo);

        if (_entries.Count > MaxEntries)
        {
            _entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);
        }
    }

    public void Clear() => _entries.Clear();
}
