using System.Net;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WoWRandomizer.Shared;
using WoWRandomizer.Web.Pages;
using WoWRandomizer.Web.Services;

namespace WoWRandomizer.Web.ComponentTests;

public class HomeTests : TestContext
{
    private static readonly ComboResponse OrcWarrior = new("Orc", "Warrior", "Horde");
    private static readonly ComboResponse HumanPaladin = new("Human", "Paladin", "Alliance");

    private readonly FakeComboApiClient _client = new();

    public HomeTests()
    {
        Services.AddSingleton<IComboApiClient>(_client);
    }

    // ---------- initial page ----------

    [Fact]
    public void Home_RendersTheHeadingTaglineAndAnEnabledRollButton()
    {
        var cut = RenderComponent<Home>();

        Assert.Equal("WoWRandomizer", cut.Find("h1").TextContent);
        Assert.Contains("Let the dice choose", cut.Markup);
        Assert.Equal("Roll", cut.Find("button.roll-button").TextContent.Trim());
        Assert.False(cut.Find("button.roll-button").HasAttribute("disabled"));
    }

    [Fact]
    public void Home_BeforeRolling_ShowsNoResultOrErrorAndDoesNotCallTheApi()
    {
        var cut = RenderComponent<Home>();

        Assert.Empty(cut.FindAll(".result"));
        Assert.Empty(cut.FindAll(".error"));
        Assert.Empty(cut.FindAll(".loading"));
        Assert.Equal(0, _client.CallCount);
    }

    // ---------- success ----------

    [Fact]
    public void Roll_WhenSuccessful_ShowsRaceClassAndFaction()
    {
        _client.EnqueueResult(OrcWarrior);
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("Orc", cut.Find(".result-race").TextContent);
            Assert.Equal("Warrior", cut.Find(".result-class").TextContent);
            Assert.Equal("Horde", cut.Find(".result-faction").TextContent);
        });
        Assert.Equal(1, _client.CallCount);
    }

    [Theory]
    [InlineData("Orc", "Warrior", "Horde", "faction-horde")]
    [InlineData("Human", "Paladin", "Alliance", "faction-alliance")]
    public void Roll_StylesTheResultByFaction(string race, string characterClass, string faction, string expectedCssClass)
    {
        _client.EnqueueResult(new ComboResponse(race, characterClass, faction));
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() => Assert.Single(cut.FindAll($".result.{expectedCssClass}")));
    }

    [Fact]
    public void Roll_Again_ReplacesThePreviousResult()
    {
        _client.EnqueueResult(OrcWarrior);
        _client.EnqueueResult(HumanPaladin);
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();
        cut.WaitForAssertion(() => Assert.Equal("Orc", cut.Find(".result-race").TextContent));
        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Single(cut.FindAll(".result"));
            Assert.Equal("Human", cut.Find(".result-race").TextContent);
            Assert.Equal("Paladin", cut.Find(".result-class").TextContent);
            Assert.Equal("Alliance", cut.Find(".result-faction").TextContent);
            Assert.DoesNotContain("Orc", cut.Markup);
        });
        Assert.Equal(2, _client.CallCount);
    }

    // ---------- loading ----------

    [Fact]
    public void Roll_WhileTheRequestIsRunning_ShowsLoadingAndDisablesTheButton()
    {
        var pending = new TaskCompletionSource<ComboResponse>();
        _client.Enqueue(() => pending.Task);
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        Assert.NotEmpty(cut.FindAll(".loading"));
        Assert.True(cut.Find("button.roll-button").HasAttribute("disabled"));
        Assert.Equal("Rolling…", cut.Find("button.roll-button").TextContent.Trim());
        Assert.Empty(cut.FindAll(".result"));

        pending.SetResult(OrcWarrior);

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(cut.FindAll(".loading"));
            Assert.False(cut.Find("button.roll-button").HasAttribute("disabled"));
            Assert.Equal("Orc", cut.Find(".result-race").TextContent);
        });
    }

    [Fact]
    public void Roll_ClickedAgainWhileRunning_DoesNotCallTheApiTwice()
    {
        var pending = new TaskCompletionSource<ComboResponse>();
        _client.Enqueue(() => pending.Task);
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();
        cut.Find("button.roll-button").Click();

        Assert.Equal(1, _client.CallCount);

        pending.SetResult(OrcWarrior);
        cut.WaitForAssertion(() => Assert.Equal("Orc", cut.Find(".result-race").TextContent));
        Assert.Equal(1, _client.CallCount);
    }

    // ---------- errors ----------

    [Fact]
    public void Roll_WhenTheApiIsUnreachable_ShowsAFriendlyMessageWithARetryButton()
    {
        _client.EnqueueFailure(new HttpRequestException("Failed to fetch"));
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("couldn't reach the server", cut.Find(".error-message").TextContent);
            Assert.Equal("Try again", cut.Find("button.retry-button").TextContent.Trim());
            Assert.Empty(cut.FindAll(".result"));
            Assert.Empty(cut.FindAll(".loading"));
        });
        Assert.DoesNotContain("Failed to fetch", cut.Markup);
    }

    [Fact]
    public void Roll_WhenTheApiReturnsAnError_ShowsAFriendlyMessageWithARetryButton()
    {
        _client.EnqueueFailure(new ComboApiException(HttpStatusCode.InternalServerError, "The API returned status code 500."));
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Something went wrong", cut.Find(".error-message").TextContent);
            Assert.NotEmpty(cut.FindAll("button.retry-button"));
            Assert.Empty(cut.FindAll(".result"));
        });
        Assert.DoesNotContain("status code 500", cut.Markup);
    }

    [Fact]
    public void Retry_AfterAnError_RollsAgainAndShowsTheResult()
    {
        _client.EnqueueFailure(new HttpRequestException("Failed to fetch"));
        _client.EnqueueResult(HumanPaladin);
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".error")));
        cut.Find("button.retry-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(cut.FindAll(".error"));
            Assert.Equal("Human", cut.Find(".result-race").TextContent);
            Assert.Equal("Paladin", cut.Find(".result-class").TextContent);
        });
        Assert.Equal(2, _client.CallCount);
    }

    [Fact]
    public void Roll_AfterASuccessfulRollThenAnError_ClearsTheOldResult()
    {
        _client.EnqueueResult(OrcWarrior);
        _client.EnqueueFailure(new HttpRequestException("Failed to fetch"));
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".result")));
        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.NotEmpty(cut.FindAll(".error"));
            Assert.Empty(cut.FindAll(".result"));
        });
    }
}
