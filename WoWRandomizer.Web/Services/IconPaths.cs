namespace WoWRandomizer.Web.Services;

/// <summary>
/// Maps a race, class or faction name to its icon under <c>wwwroot/icons</c>.
/// The file name is the name in lower case with spaces as dashes, for example <c>icons/races/night-elf.png</c>.
/// </summary>
public static class IconPaths
{
    public static string Race(string name) => Build("races", name);

    public static string Class(string name) => Build("classes", name);

    public static string Faction(string name) => Build("factions", name);

    private static string Build(string folder, string name) =>
        $"icons/{folder}/{string.Join('-', name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToLowerInvariant()}.png";
}
