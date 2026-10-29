using Bunit;
using WoWRandomizer.Web.Layout;

namespace WoWRandomizer.Web.ComponentTests;

public class MainLayoutTests : TestContext
{
    [Fact]
    public void MainLayout_RendersItsBodyInsideTheMainElement()
    {
        var cut = RenderComponent<MainLayout>(parameters => parameters
            .Add(layout => layout.Body, "<p id=\"child\">hello</p>"));

        Assert.Equal("hello", cut.Find("main.app #child").TextContent);
    }

    [Fact]
    public void MainLayout_HasAHeaderWithTheAppName()
    {
        var cut = RenderComponent<MainLayout>(parameters => parameters.Add(layout => layout.Body, "<p>hello</p>"));

        Assert.Equal("WoWRandomizer", cut.Find("header .brand").TextContent);
    }

    [Fact]
    public void MainLayout_HasAFooterWithTheBlizzardDisclaimer()
    {
        var cut = RenderComponent<MainLayout>(parameters => parameters.Add(layout => layout.Body, "<p>hello</p>"));

        Assert.Contains("not affiliated with Blizzard Entertainment", cut.Find("footer").TextContent);
    }

    [Fact]
    public void MainLayout_OffersASkipLinkToTheMainContent()
    {
        var cut = RenderComponent<MainLayout>(parameters => parameters.Add(layout => layout.Body, "<p>hello</p>"));

        var target = cut.Find("a.skip-link").GetAttribute("href")!.TrimStart('#');

        Assert.Equal("main-content", target);
        Assert.NotNull(cut.Find($"main#{target}"));
    }
}
