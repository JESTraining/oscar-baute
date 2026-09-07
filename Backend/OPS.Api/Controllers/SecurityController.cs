using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Application.Dtos;
using OPS.Application.Interfaces;

namespace OPS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SecurityController(ISecurityService securityService) : ControllerBase
{
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate(AuthenticateRequest request, CancellationToken cancellationToken)
    {
        var result = await securityService.Authenticate(request, cancellationToken);


        if (result is null)
        {
            return Unauthorized();
        }


        return Ok(result);
    }
}
