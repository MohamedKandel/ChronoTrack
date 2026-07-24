using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]/api")]
public class ProfileController : ControllerBase
{
    private readonly ProfileService _service;

    public ProfileController(ProfileService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpGet("getProfile")]
    public async Task<IActionResult> GetProfile()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _service.GetProfile(baseUrl,accountIdClaim);
        switch(result.status)
        {
            case ResponseStatus.NotVerified:
                return StatusCode(403, new GeneralResponse<String>
                {
                    code = 403,
                    message = "Account didn't verified yet, please verify it first"
                });
            
            case ResponseStatus.NotFound:
                return NotFound(new GeneralResponse<String>
                {
                    code = 404,
                    message = "Account not found"
                });
            case ResponseStatus.Success:
                return Ok(new GeneralResponse<ProfileDTO>
                {
                    code = 200,
                    message = $"Hello {result.Username}",
                    data = result
                });
            default:
                return BadRequest();
        }
    }

    [HttpGet("images/{id}")]
    public async Task<IActionResult> GetImage(string id)
    {
        var result = await _service.GetImage(id);

        if (result.Image == null)
            return NotFound();

        return File(result.Image, result.ContentType ?? "image/jpeg");
    }
}