using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Common;
using OnePortal.Application.Lookups.Dtos;
using OnePortal.Application.Users.Dtos;

namespace OnePortal.API.Controllers
{
   [ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly ILookupRepository _lookups;
    public LookupsController(ILookupRepository lookups) => _lookups = lookups;

    [HttpGet]
   [Authorize]
   public async Task<ActionResult<ApiResponse<LookupSetsDto>>> GetAll(CancellationToken ct)
        {
            var ok =  await _lookups.GetAllAsync(ct);
            return Ok(ApiResponse<LookupSetsDto>.Success(ok));
        }
        
}
}
