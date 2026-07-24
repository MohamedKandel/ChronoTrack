using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]/api")]
public class AuthController : ControllerBase
{
    private readonly AuthService _service;

    public AuthController(AuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterAccountDTO model)
    {
        var result = await _service.RegisterAccount(model);
        switch (result)
        {
            case ResponseStatus.OTPNotSent:
                return BadRequest(new GeneralResponse<String>
                {
                    code = 500,
                    message = "Account created but failed to send the OTP\nPlease contact the system administartor"
                });
            case ResponseStatus.Success:
                return StatusCode(201, new GeneralResponse<string>
                {
                    code = 201,
                    message = "Account created successfully"
                });
            default:
                return BadRequest();
        }
    }

    [HttpPost("verifyOTP")]
    public async Task<IActionResult> VerifyOTP([FromBody] VerifyDTO verifyDTO)
    {
        var result = await _service.VerifyOtpAsync(verifyDTO);
        switch(result)
        {
            case ResponseStatus.InvalidOtp:
                return StatusCode(403,new GeneralResponse<String>
                {
                    code = 403,
                    message = "OTP is invalid or expired"
                });
            case ResponseStatus.NotFound:
                return NotFound(new GeneralResponse<String>
                {
                    code = 404,
                    message = "User not found with this email"
                });
            case ResponseStatus.Success:
                return Ok(new GeneralResponse<String>
                {
                    code = 200,
                    message = "Account verified successfully"
                });
            default:
                return BadRequest();
        }
    }

    [HttpPost("forgetPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ResendOTPDTO model)
    {
        var result = await _service.ForgetPassword(model);
        switch(result)
        {
            case ResponseStatus.NotFound:
                return NotFound(new GeneralResponse<String>
                {
                    code = 404,
                    message = "User not found with this email"
                });
            case ResponseStatus.Success:
                return Ok(new GeneralResponse<String>
                {
                    code = 200,
                    message = "There is an OTP sent to your email, please check and verify that's you"
                });
            default:
                return BadRequest();
        }
    }

    [HttpPost("resendOTP")]
    public async Task<IActionResult> ResendOTP([FromBody] ResendOTPDTO model)
    {
        var result = await _service.ResendOTP(model);
        switch(result)
        {
            case ResponseStatus.NotFound:
                return NotFound(new GeneralResponse<String>
                {
                    code = 404,
                    message = "User not found with this email"
                });
            case ResponseStatus.Success:
                return Ok(new GeneralResponse<String>
                {
                    code = 200,
                    message = $"A new OTP sent successfully to your email {model.email}".Trim()
                });
            default:
                return BadRequest();
        }
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
    {
        var result = await _service.ResetPassword(model);
        switch(result)
        {
            case ResponseStatus.NotVerified:
                return StatusCode(403, new GeneralResponse<String>
                {
                    code = 403,
                    message = "This email didn't verify yet, please verify it first before reseting password"
                });
            case ResponseStatus.NotFound:
                return NotFound(new GeneralResponse<String>
                {
                    code = 404,
                    message = "User not found with this email"
                });
            case ResponseStatus.Success:
                return Ok(new GeneralResponse<String>
                {
                    code = 200,
                    message = "Password changed successfully"
                });
            default:
                return BadRequest();
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO login)
    {
        var result = await _service.Login(login);
        switch(result.status)
        {
            case ResponseStatus.NotVerified:
                return StatusCode(403, new GeneralResponse<String>
                {
                    code = 403,
                    message = "This email didn't verify yet, please verify it first before logging in"
                });
            case ResponseStatus.NotFound:
            case ResponseStatus.InvalidPassword:
                return NotFound(new GeneralResponse<String>
                {
                    code = 404,
                    message = "Incorrect email or password"
                });
            case ResponseStatus.Success:
                return Ok(new GeneralResponse<String>
                {
                    code = 200,
                    message = "Logged in successfully",
                    data = result.token
                });
            default:
                return BadRequest();
        }
    }
}