using Microsoft.AspNetCore.Mvc;
using WoWRandomizer.API.Models;
using WoWRandomizer.Domain;

namespace WoWRandomizer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComboController : ControllerBase
{
    private readonly IComboService _comboService;

    public ComboController(IComboService comboService)
    {
        _comboService = comboService;
    }

    /// <summary>Returns a random valid race + class combo, optionally limited to one faction.</summary>
    /// <param name="faction">Optional faction (horde or alliance, case-insensitive). When omitted, any faction can be returned.</param>
    [HttpGet]
    [ProducesResponseType<ComboResponse>(StatusCodes.Status200OK)]
    public ActionResult<ComboResponse> Get([FromQuery] Faction? faction)
    {
        var combo = faction is null
            ? _comboService.GetRandom()
            : _comboService.GetRandom(faction.Value);

        return ComboResponse.From(combo);
    }
}
