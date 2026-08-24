using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Server.Services;
using POS.Shared.DTOs;
using System.Security.Claims;

namespace POS.Server.Controllers;

[ApiController, Route("api/receptions"), Authorize]
public class ReceptionsController(IReceptionService service) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
    public Task<List<ReceptionResponse>> Get() => service.GetAllAsync();

    [HttpGet("{id:int}"), Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
    public async Task<ActionResult<ReceptionResponse>> Get(int id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost, Authorize(Roles = "Administrador,Almacén")]
    public async Task<ActionResult<ReceptionResponse>> Post(CreateReceptionRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try
        {
            var result = await service.CreateAsync(request, userId);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (BusinessValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPatch("{id:int}/confirm"), Authorize(Roles = "Administrador,Almacén")]
    public async Task<ActionResult<ReceptionResponse>> Confirm(int id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try
        {
            var result = await service.ConfirmAsync(id, userId);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BusinessValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    private bool TryGetUserId(out int id) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id);
}
