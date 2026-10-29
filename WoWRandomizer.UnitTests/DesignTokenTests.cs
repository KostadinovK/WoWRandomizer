using System.Globalization;
using System.Text.RegularExpressions;
using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;
using WoWRandomizer.Web.Services;

namespace WoWRandomizer.UnitTests;

public partial class DesignTokenTests
{
    private const double MinimumTextContrast = 4.5;

    private static readonly Lazy<string> Css = new(() => File.ReadAllText(Path.Combine(WebAssets.FindWwwroot(), "css", "app.css")));

    private static string RealDataPath => Path.Combine(AppContext.BaseDirectory, "Data", "combos.json");

    [Fact]
    public void EveryClassInTheData_HasAClassColourToken()
    {
        var classes = JsonComboProvider.FromFile(RealDataPath).GetAll().Select(combo => combo.CharacterClass).Distinct().ToList();

        Assert.Equal(9, classes.Count);
        Assert.All(classes, characterClass =>
        {
            var slug = IconPaths.Slug(characterClass);

            Assert.True(Tokens().ContainsKey($"class-{slug}"), $"Missing --class-{slug}");
            Assert.Contains($".class-{slug}", Css.Value);
        });
    }

    [Fact]
    public void EveryFaction_HasFillAndTextTokens()
    {
        Assert.All(Enum.GetValues<Faction>().Select(faction => faction.ToString().ToLowerInvariant()), faction =>
        {
            Assert.True(Tokens().ContainsKey(faction), $"Missing --{faction}");
            Assert.True(Tokens().ContainsKey($"{faction}-text"), $"Missing --{faction}-text");
        });
    }

    [Fact]
    public void TheStylesheet_DisablesAnimationsForReducedMotion()
    {
        Assert.Matches(@"@media \(prefers-reduced-motion: reduce\)[^@]*animation: none", Css.Value);
    }

    [Theory]
    [InlineData("text")]
    [InlineData("text-muted")]
    [InlineData("horde-text")]
    [InlineData("alliance-text")]
    [InlineData("class-warrior")]
    [InlineData("class-paladin")]
    [InlineData("class-hunter")]
    [InlineData("class-rogue")]
    [InlineData("class-priest")]
    [InlineData("class-shaman-text")]
    [InlineData("class-mage")]
    [InlineData("class-warlock")]
    [InlineData("class-druid")]
    public void TextColours_HaveEnoughContrastOnEverySurface(string token)
    {
        var text = Tokens()[token];

        Assert.All(new[] { "bg", "surface", "surface-raised" }, surface =>
        {
            var contrast = Contrast(text, Tokens()[surface]);

            Assert.True(contrast >= MinimumTextContrast, $"--{token} on --{surface} is {contrast:0.0}:1");
        });
    }

    [Fact]
    public void ButtonTextColours_HaveEnoughContrastOnTheirFills()
    {
        Assert.True(Contrast(Tokens()["on-gold"], Tokens()["gold"]) >= MinimumTextContrast);
        Assert.True(Contrast("#ffffff", Tokens()["horde"]) >= MinimumTextContrast);
        Assert.True(Contrast("#ffffff", Tokens()["alliance"]) >= MinimumTextContrast);
    }

    /// <summary>Reads the <c>--name: #rrggbb;</c> colour tokens defined in the stylesheet.</summary>
    private static Dictionary<string, string> Tokens() => TokenPattern().Matches(Css.Value)
        .ToDictionary(match => match.Groups["name"].Value, match => match.Groups["hex"].Value, StringComparer.Ordinal);

    [GeneratedRegex(@"--(?<name>[a-z-]+):\s*(?<hex>#[0-9a-fA-F]{6})\s*;")]
    private static partial Regex TokenPattern();

    /// <summary>WCAG 2 contrast ratio between two hex colours.</summary>
    private static double Contrast(string first, string second)
    {
        var (a, b) = (Luminance(first), Luminance(second));

        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    private static double Luminance(string hex)
    {
        double Channel(int start)
        {
            var value = int.Parse(hex.AsSpan(start, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;

            return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(1) + 0.7152 * Channel(3) + 0.0722 * Channel(5);
    }
}
