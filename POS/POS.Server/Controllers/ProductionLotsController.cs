using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Server.Services;
using POS.Shared.DTOs;
using System.Security.Claims;

namespace POS.Server.Controllers;

[ApiController, Route("api/production-lots"), Authorize]
public class ProductionLotsController(IProductionLotService service) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
    public Task<List<ProductionLotResponse>> Get() => service.GetAllAsync();
    [HttpGet("{id:int}"), Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
    public async Task<ActionResult<ProductionLotResponse>> Get(int id) => (await service.GetByIdAsync(id)) is { } result ? Ok(result) : NotFound();
    [HttpPost, Authorize(Roles = "Administrador,Producción")]
    public async Task<ActionResult<ProductionLotResponse>> Post(CreateProductionLotRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { var result = await service.CreateAsync(request, userId); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }
        catch (BusinessValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }
    [HttpPost("{id:int}/consume"), Authorize(Roles = "Administrador,Producción")]
    public async Task<ActionResult<ProductionLotResponse>> Consume(int id, MaterialConsumptionRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return (await service.ConsumeAsync(id, request, userId)) is { } result ? Ok(result) : NotFound(); }
        catch (BusinessValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }
    [HttpPost("{id:int}/start"), Authorize(Roles = "Administrador,Producción")]
    public async Task<ActionResult<ProductionLotResponse>> Start(int id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return (await service.StartAsync(id, userId)) is { } result ? Ok(result) : NotFound(); }
        catch (BusinessValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }
    [HttpPost("{id:int}/finish"), Authorize(Roles = "Administrador,Producción")]
    public async Task<ActionResult<ProductionLotResponse>> Finish(int id, FinishProductionLotRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return (await service.FinishAsync(id, request, userId)) is { } result ? Ok(result) : NotFound(); }
        catch (BusinessValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }
    private bool TryGetUserId(out int id) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id);
}
