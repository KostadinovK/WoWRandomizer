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
    /// <param name="faction">Optional faction (horde or alliance, case-insensitive). When omitted or empty, any faction can be returned.</param>
    [HttpGet]
    [ProducesResponseType<ComboResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<ComboResponse> Get([FromQuery] string? faction)
    {
        if (string.IsNullOrWhiteSpace(faction))
        {
            return ComboResponse.From(_comboService.GetRandom());
        }

        if (!FactionParser.TryParse(faction, out var parsedFaction))
        {
            return Problem(
                title: "Invalid faction",
                detail: $"'{faction}' is not a valid faction. Accepted values: {string.Join(", ", FactionParser.AcceptedValues)}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return ComboResponse.From(_comboService.GetRandom(parsedFaction));
    }
}
