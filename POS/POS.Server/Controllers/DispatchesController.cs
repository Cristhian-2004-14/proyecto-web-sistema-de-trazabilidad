using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using POS.Server.Services;using POS.Shared.DTOs;using System.Security.Claims;
namespace POS.Server.Controllers;
[ApiController,Route("api/dispatches"),Authorize] public class DispatchesController(IDispatchService service):ControllerBase
{
 [HttpGet,Authorize(Roles="Administrador,Almacén,Producción,Gerencia")]public Task<List<DispatchResponse>>Get()=>service.GetAllAsync();
 [HttpGet("{id:int}"),Authorize(Roles="Administrador,Almacén,Producción,Gerencia")]public async Task<ActionResult<DispatchResponse>>Get(int id)=>(await service.GetByIdAsync(id))is{}x?Ok(x):NotFound();
 [HttpPost,Authorize(Roles="Administrador,Almacén")]public async Task<ActionResult<DispatchResponse>>Post(CreateDispatchRequest r){if(!UserId(out var id))return Unauthorized();try{var x=await service.CreateAsync(r,id);return CreatedAtAction(nameof(Get),new{id=x.Id},x);}catch(BusinessValidationException e){return BadRequest(new{message=e.Message});}}
 [HttpPatch("{id:int}/confirm"),Authorize(Roles="Administrador,Almacén")]public async Task<ActionResult<DispatchResponse>>Confirm(int id){if(!UserId(out var uid))return Unauthorized();try{return(await service.ConfirmAsync(id,uid))is{}x?Ok(x):NotFound();}catch(BusinessValidationException e){return BadRequest(new{message=e.Message});}}
 private bool UserId(out int id)=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out id);
}
