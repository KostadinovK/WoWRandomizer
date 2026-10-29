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
        Services.AddSingleton<IRollHistory, RollHistory>();
    }

    // ---------- initial page ----------

    [Fact]
    public void Home_RendersTheHeadingTaglineAndAnEnabledRollButton()
    {
        var cut = RenderComponent<Home>();

        Assert.Equal("WoWRandomizer", cut.Find("h1").TextContent);
        Assert.Contains("Let the fate choose", cut.Markup);
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

    // ---------- faction selector ----------

    [Fact]
    public void FactionSelector_RendersAnAccessibleGroupWithHordeAndAllianceOnly()
    {
        var cut = RenderComponent<Home>();

        var group = cut.Find("[role=group]");
        var labelId = group.GetAttribute("aria-labelledby");
        Assert.Equal("Faction", cut.Find($"#{labelId}").TextContent);

        var options = group.QuerySelectorAll("button.faction-option").ToList();
        Assert.Equal(["Horde", "Alliance"], options.Select(option => option.TextContent.Trim()));
        Assert.All(options, option =>
        {
            Assert.Equal("false", option.GetAttribute("aria-pressed"));
            Assert.NotNull(option.QuerySelector(".game-icon img"));
        });
    }

    [Fact]
    public void Roll_WithNoFactionSelected_SendsNoFaction()
    {
        _client.EnqueueResult(OrcWarrior);
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".result")));
        Assert.Equal([null], _client.Factions);
    }

    [Theory]
    [InlineData("Horde", "horde")]
    [InlineData("Alliance", "alliance")]
    public void Roll_WithAFactionSelected_SendsThatFaction(string label, string expectedFaction)
    {
        _client.EnqueueResult(OrcWarrior);
        var cut = RenderComponent<Home>();

        FactionButton(cut, label).Click();
        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".result")));
        Assert.Equal([expectedFaction], _client.Factions);
        Assert.Equal("true", FactionButton(cut, label).GetAttribute("aria-pressed"));
        Assert.Contains("selected", FactionButton(cut, label).ClassList);
    }

    [Fact]
    public void FactionSelector_SelectingTheOtherFaction_LeavesOnlyTheLatestSelected()
    {
        var cut = RenderComponent<Home>();

        FactionButton(cut, "Horde").Click();
        FactionButton(cut, "Alliance").Click();

        Assert.Equal("false", FactionButton(cut, "Horde").GetAttribute("aria-pressed"));
        Assert.Equal("true", FactionButton(cut, "Alliance").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void FactionSelector_ClickingTheSelectedFactionAgain_ClearsItAndRollsWithAnyFaction()
    {
        _client.EnqueueResult(OrcWarrior);
        var cut = RenderComponent<Home>();

        FactionButton(cut, "Horde").Click();
        FactionButton(cut, "Horde").Click();
        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".result")));
        Assert.Equal("false", FactionButton(cut, "Horde").GetAttribute("aria-pressed"));
        Assert.Equal([null], _client.Factions);
    }

    [Fact]
    public void FactionSelector_KeepsTheSelectionWhenRollingAgainAndAfterAnError()
    {
        _client.EnqueueResult(OrcWarrior);
        _client.EnqueueFailure(new HttpRequestException("Failed to fetch"));
        _client.EnqueueResult(OrcWarrior);
        var cut = RenderComponent<Home>();

        FactionButton(cut, "Horde").Click();
        cut.Find("button.roll-button").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".result")));
        cut.Find("button.roll-button").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".error")));
        cut.Find("button.retry-button").Click();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".result")));

        Assert.Equal(["horde", "horde", "horde"], _client.Factions);
        Assert.Equal("true", FactionButton(cut, "Horde").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void FactionSelector_WhileTheRequestIsRunning_IsDisabled()
    {
        var pending = new TaskCompletionSource<ComboResponse>();
        _client.Enqueue(() => pending.Task);
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        Assert.All(cut.FindAll("button.faction-option"), option => Assert.True(option.HasAttribute("disabled")));

        pending.SetResult(OrcWarrior);

        cut.WaitForAssertion(() =>
            Assert.All(cut.FindAll("button.faction-option"), option => Assert.False(option.HasAttribute("disabled"))));
    }

    private static AngleSharp.Dom.IElement FactionButton(IRenderedComponent<Home> cut, string label) =>
        cut.FindAll("button.faction-option").Single(option => option.TextContent.Trim() == label);

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
            Assert.Equal("Horde", cut.Find(".result-faction-name").TextContent);
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

    // ---------- history ----------

    private void RollOnce(IRenderedComponent<Home> cut, ComboResponse combo, int expectedCalls)
    {
        _client.EnqueueResult(combo);
        cut.Find("button.roll-button").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Equal(expectedCalls, _client.CallCount);
            Assert.False(cut.Find("button.roll-button").HasAttribute("disabled"));
        });
    }

    [Fact]
    public void History_BeforeTheFirstRoll_IsNotShown()
    {
        var cut = RenderComponent<Home>();

        Assert.Empty(cut.FindAll(".history"));
        Assert.Empty(cut.FindAll(".clear-history-button"));
    }

    [Fact]
    public void History_ListsTheRollsNewestFirstWithIcons()
    {
        var cut = RenderComponent<Home>();

        RollOnce(cut, OrcWarrior, 1);
        RollOnce(cut, new ComboResponse("Night Elf", "Druid", "Alliance"), 2);

        var entries = cut.FindAll(".history-entry");
        Assert.Equal(2, entries.Count);
        Assert.Equal("Night Elf", entries[0].QuerySelector(".history-race")!.TextContent);
        Assert.Equal("Druid", entries[0].QuerySelector(".history-class")!.TextContent);
        Assert.Equal("Alliance", entries[0].QuerySelector(".history-faction-name")!.TextContent);
        Assert.Contains("faction-alliance", entries[0].ClassList);
        Assert.Equal("Orc", entries[1].QuerySelector(".history-race")!.TextContent);
        Assert.Equal(
            ["icons/races/night-elf.png", "icons/classes/druid.png", "icons/factions/alliance.png"],
            entries[0].QuerySelectorAll("img").Select(img => img.GetAttribute("src")));
        Assert.Equal(["Night Elf", "Druid", "Alliance"], entries[0].QuerySelectorAll("img").Select(img => img.GetAttribute("alt")));
    }

    [Fact]
    public void History_ShowsAtMostTheLastTenRolls()
    {
        var cut = RenderComponent<Home>();

        foreach (var number in Enumerable.Range(1, 12))
        {
            RollOnce(cut, new ComboResponse($"Race{number}", "Warrior", "Horde"), number);
        }

        var races = cut.FindAll(".history-race").Select(race => race.TextContent).ToList();
        Assert.Equal(10, races.Count);
        Assert.Equal("Race12", races[0]);
        Assert.Equal("Race3", races[^1]);
    }

    [Fact]
    public void History_WhenARollFails_DoesNotAddAnEntry()
    {
        var cut = RenderComponent<Home>();
        RollOnce(cut, OrcWarrior, 1);
        _client.EnqueueFailure(new HttpRequestException("Failed to fetch"));

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".error")));
        Assert.Single(cut.FindAll(".history-entry"));
    }

    [Fact]
    public void ClearHistory_RemovesTheListButKeepsTheCurrentResult()
    {
        var cut = RenderComponent<Home>();
        RollOnce(cut, OrcWarrior, 1);
        RollOnce(cut, HumanPaladin, 2);

        cut.Find("button.clear-history-button").Click();

        Assert.Empty(cut.FindAll(".history"));
        Assert.Empty(cut.FindAll(".clear-history-button"));
        Assert.Equal("Human", cut.Find(".result-race").TextContent);
    }

    [Fact]
    public void ClearHistory_ThenRolling_StartsAFreshList()
    {
        var cut = RenderComponent<Home>();
        RollOnce(cut, OrcWarrior, 1);
        cut.Find("button.clear-history-button").Click();

        RollOnce(cut, HumanPaladin, 2);

        var races = cut.FindAll(".history-race").Select(race => race.TextContent);
        Assert.Equal(["Human"], races);
    }

    // ---------- icons ----------

    [Fact]
    public void Roll_ShowsTheRaceClassAndFactionIconsWithAltText()
    {
        _client.EnqueueResult(new ComboResponse("Night Elf", "Druid", "Alliance"));
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() =>
        {
            var icons = cut.FindAll(".result img").Select(img => (img.GetAttribute("src"), img.GetAttribute("alt"))).ToList();
            Assert.Equal(
                [
                    ("icons/races/night-elf.png", "Night Elf"),
                    ("icons/classes/druid.png", "Druid"),
                    ("icons/factions/alliance.png", "Alliance")
                ],
                icons);
        });
    }

    [Fact]
    public void Roll_GivesEveryIconAHiddenInitialFallbackForAMissingFile()
    {
        _client.EnqueueResult(new ComboResponse("Orc", "Warrior", "Horde"));
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() =>
        {
            var icons = cut.FindAll(".result .game-icon");
            Assert.Equal(["O", "W", "H"], icons.Select(icon => icon.QuerySelector(".game-icon-fallback")!.GetAttribute("data-initial")));
            Assert.All(icons, icon =>
            {
                Assert.True(icon.QuerySelector(".game-icon-fallback")!.HasAttribute("hidden"));
                Assert.Contains("this.hidden = true", icon.QuerySelector("img")!.GetAttribute("onerror"));
            });
        });
    }

    [Fact]
    public void Roll_ForARaceWithoutAnIconFile_StillRendersTheResult()
    {
        _client.EnqueueResult(new ComboResponse("Blood Elf", "Paladin", "Horde"));
        var cut = RenderComponent<Home>();

        cut.Find("button.roll-button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("Blood Elf", cut.Find(".result-race").TextContent);
            Assert.Equal("icons/races/blood-elf.png", cut.Find(".result img").GetAttribute("src"));
            Assert.Equal("B", cut.Find(".result .game-icon-fallback").GetAttribute("data-initial"));
        });
    }

    [Fact]
    public void FactionButtons_ShowTheFactionIcons()
    {
        var cut = RenderComponent<Home>();

        var sources = cut.FindAll("button.faction-option img").Select(img => img.GetAttribute("src"));

        Assert.Equal(["icons/factions/horde.png", "icons/factions/alliance.png"], sources);
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
            Assert.Equal("Alliance", cut.Find(".result-faction-name").TextContent);
            Assert.DoesNotContain("Orc", cut.Find(".result").InnerHtml);
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
