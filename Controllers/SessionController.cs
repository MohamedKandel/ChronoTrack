using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]/api")]
public class SessionController : ControllerBase
{
    private readonly SessionService _service;

    public SessionController(SessionService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost("addSession")]
    public async Task<IActionResult> AddSession([FromBody] AddSessionDTO dto)
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.AddSession(dto,accountIdClaim);
        switch(result)
        {
            case ResponseStatus.NotFound:
                return NotFound(new GeneralResponse<String>
                {
                    code = 404,
                    message = "Account not found"
                });
            case ResponseStatus.Success:
                return StatusCode(201, new GeneralResponse<String>
                {
                    code = 201,
                    message = "Session added successfully"
                });
            default:
                return BadRequest();
        }
    }

    [Authorize]
    [HttpGet("getSessions")]
    public async Task<IActionResult> GetSessionsHome([FromQuery] PaginationParams paginationParams, [FromQuery] FilteredSessions filtered)
    {
        var accountID = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.GetFilteredSessions(paginationParams,accountID,filtered);
        return Ok(result);
    }

    [Authorize]
    [HttpPatch("endSession")]
    public async Task<IActionResult> EndSession([FromQuery] string sessionID)
    {
        var result = await _service.EndSession(sessionID);

        switch(result)
        {
            case ResponseStatus.Invalid:
                return StatusCode(403, new GeneralResponse<String>
                {
                    code = 403,
                    message = "Invalid Session ID (already ended before)"
                });
            case ResponseStatus.OldSession:
                return StatusCode(403, new GeneralResponse<String>
                {
                    code = 403,
                    message = "This session is in past, contact the system administrator to end it for you"
                });
            case ResponseStatus.NotFound:
                return NotFound(new GeneralResponse<String>
                {
                    code = 404,
                    message = "There is no session with this ID"
                });
            case ResponseStatus.Success:
                return Ok(new GeneralResponse<String>
                {
                    code = 200,
                    message = "Session Ended successfully"
                });
            default:
                return BadRequest();
        }
    }

    [Authorize]
    [HttpGet("sessionStatistics")]
    public async Task<IActionResult> GetSessionStatistics()
    {
        var accountID = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.GetStatistics(accountID);
        return Ok(result);
    }
}