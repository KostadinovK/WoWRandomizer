using Bunit;
using WoWRandomizer.Web.Pages;

namespace WoWRandomizer.Web.ComponentTests;

public class HomeTests : TestContext
{
    [Fact]
    public void Home_RendersTheHeading()
    {
        var cut = RenderComponent<Home>();

        Assert.Equal("WoWRandomizer", cut.Find("h1").TextContent);
    }

    [Fact]
    public void Home_RendersTheTagline()
    {
        var cut = RenderComponent<Home>();

        Assert.Contains("Let the dice choose", cut.Markup);
    }
}
