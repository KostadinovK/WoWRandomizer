using WoWRandomizer.Shared;

namespace WoWRandomizer.Web.Services;

/// <summary>The most recent rolls of the current browser session, newest first.</summary>
public interface IRollHistory
{
    /// <summary>The kept rolls, newest first. At most <see cref="RollHistory.MaxEntries"/>.</summary>
    IReadOnlyList<ComboResponse> Entries { get; }

    /// <summary>Records a roll as the newest entry; the oldest entry is dropped when the history is full.</summary>
    void Add(ComboResponse combo);

    /// <summary>Removes every entry.</summary>
    void Clear();
}
