using Microsoft.AspNetCore.Mvc;
using WoWRandomizer.API.Models;
using WoWRandomizer.Domain;

namespace WoWRandomizer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComboController : ControllerBase
{
    private readonly ComboService _comboService;

    public ComboController(ComboService comboService)
    {
        _comboService = comboService;
    }

    /// <summary>Returns a random valid race + class combo.</summary>
    [HttpGet]
    [ProducesResponseType<ComboResponse>(StatusCodes.Status200OK)]
    public ActionResult<ComboResponse> Get() =>
        ComboResponse.From(_comboService.GetRandom());
}
