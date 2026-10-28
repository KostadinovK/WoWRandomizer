using Microsoft.AspNetCore.Mvc;
using WoWRandomizer.API.Models;
using WoWRandomizer.Domain;
using WoWRandomizer.Shared;

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

    /// <summary>Returns a random valid race + class combo.</summary>
    /// <remarks>Every valid combo is equally likely. Optionally limit the result to one faction.</remarks>
    /// <param name="faction" example="horde">Optional faction: <c>horde</c> or <c>alliance</c> (case-insensitive). When omitted or empty, any faction can be returned.</param>
    /// <response code="200">A random valid combo.</response>
    /// <response code="400">The faction is not <c>horde</c> or <c>alliance</c>. The response lists the accepted values.</response>
    [HttpGet]
    [ProducesResponseType<ComboResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public ActionResult<ComboResponse> Get([FromQuery] string? faction)
    {
        if (string.IsNullOrWhiteSpace(faction))
        {
            return _comboService.GetRandom().ToResponse();
        }

        if (!FactionParser.TryParse(faction, out var parsedFaction))
        {
            return Problem(
                title: "Invalid faction",
                detail: $"'{faction}' is not a valid faction. Accepted values: {string.Join(", ", FactionParser.AcceptedValues)}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return _comboService.GetRandom(parsedFaction).ToResponse();
    }
}
