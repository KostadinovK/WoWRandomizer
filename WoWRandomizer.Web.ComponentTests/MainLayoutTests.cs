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
}
