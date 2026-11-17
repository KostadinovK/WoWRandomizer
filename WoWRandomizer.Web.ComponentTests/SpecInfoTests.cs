using Bunit;
using WoWRandomizer.Shared;
using WoWRandomizer.Web.Components;

namespace WoWRandomizer.Web.ComponentTests;

public class SpecInfoTests : TestContext
{
    [Fact]
    public void SpecInfo_ShowsTheNameAndASingleTypeWithoutASeparator()
    {
        var cut = RenderComponent<SpecInfo>(parameters => parameters
            .Add(component => component.Spec, new SpecResponse("Holy", ["Healer"]))
            .Add(component => component.Prefix, "result"));

        Assert.Equal("Holy", cut.Find(".result-spec-name").TextContent);
        Assert.Equal(["Healer"], cut.FindAll(".spec-type").Select(type => type.TextContent));
        Assert.Empty(cut.FindAll(".spec-type-separator"));
    }

    [Fact]
    public void SpecInfo_WithSeveralTypes_SeparatesThemAndTagsEachWithItsOwnClass()
    {
        var cut = RenderComponent<SpecInfo>(parameters => parameters
            .Add(component => component.Spec, new SpecResponse("Feral Combat", ["Tank", "DPS"]))
            .Add(component => component.Prefix, "history"));

        Assert.Equal("Feral Combat", cut.Find(".history-spec-name").TextContent);
        Assert.Equal(["Tank", "DPS"], cut.FindAll(".spec-type").Select(type => type.TextContent));
        Assert.Single(cut.FindAll(".spec-type-separator"));
        Assert.NotEmpty(cut.FindAll(".spec-type-tank"));
        Assert.NotEmpty(cut.FindAll(".spec-type-dps"));
    }
}
