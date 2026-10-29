namespace WoWRandomizer.UnitTests;

internal static class WebAssets
{
    /// <summary>Walks up from the test output folder to the repository root and returns the web project's wwwroot.</summary>
    public static string FindWwwroot()
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
