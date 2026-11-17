using WoWRandomizer.API.Models;
using WoWRandomizer.Domain;
using WoWRandomizer.Shared;

namespace WoWRandomizer.IntegrationTests;

public class ComboMappingTests
{
    [Fact]
    public void ToResponse_MapsRaceClassFactionAndSpec()
    {
        var combo = new Combo("Orc", "Warrior", Faction.Horde, new Spec("Fury", SpecType.DPS));

        var response = combo.ToResponse();

        Assert.Equal(new ComboResponse("Orc", "Warrior", "Horde", new SpecResponse("Fury", ["DPS"])), response);
    }

    [Fact]
    public void ToResponse_SpecWithSeveralTypes_ListsEveryTypeAsText()
    {
        var combo = new Combo("Tauren", "Druid", Faction.Horde, new Spec("Feral Combat", SpecType.Tank, SpecType.DPS));

        var response = combo.ToResponse();

        Assert.Equal(["Tank", "DPS"], response.Spec.Types);
    }
}
