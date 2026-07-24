using Microsoft.EntityFrameworkCore;

public class AuthService
{
    private readonly EmailService _emailService;
    private readonly JWTService _jwtService;
    private readonly AppDBContext _context;

    public AuthService(EmailService emailService, JWTService jwtService, AppDBContext context)
    {
        _context = context;
        _emailService = emailService;
        _jwtService = jwtService;
    }

    private async Task<string> GenerateUniqueAccountId()
    {
        string userId;
        do
        {
            userId = Utils.GenerateId();
        } while (await _context.Accounts.AnyAsync(x => x.AccountID == userId));

        return userId;
    }

    private async Task<bool> SendOtpAsync(Account account)
    {
        if (account.LastTimeRequested != null &&
            account.LastTimeRequested > DateTime.Now.AddMinutes(-1))
        {
            return false; // OTP request too soon
        }

        // Generate OTP
        string otp = Utils.GenerateOTP();
        account.OTP = otp;
        account.ExpiryAt = DateTime.Now.AddMinutes(5);
        account.LastTimeRequested = DateTime.Now;

        await _context.SaveChangesAsync();
        _emailService.SendEmail(
            account.Email,
            "Activation Code",
            $"{Utils.GenerateOtpEmailTemplate(otp, "ChronoTrack")}"
        );
        return true;
    }

    public async Task<ResponseStatus> VerifyOtpAsync(VerifyDTO verifyOtpDTO)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == verifyOtpDTO.email);

        if (account == null)
        {
            return ResponseStatus.NotFound;
        }

        if (account.OTP != verifyOtpDTO.OTP || account.ExpiryAt < DateTime.Now)
        {
            return ResponseStatus.InvalidOtp;
        }

        account.IsVerified = true;
        account.OTP = null;
        account.ExpiryAt = null;

        await _context.SaveChangesAsync();

        return ResponseStatus.Success;

    }

    public async Task<ResponseStatus> ResendOTP(ResendOTPDTO account)
    {
        var acc = await _context.Accounts
            .FirstOrDefaultAsync(x => x.Email == account.email);

        if (acc == null)
        {
            return ResponseStatus.NotFound;
        }
        

        await SendOtpAsync(acc);

        return ResponseStatus.Success;
    }

    public async Task<ResponseStatus> ForgetPassword(ResendOTPDTO dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == dto.email);

        if (account == null)
        {
            return ResponseStatus.NotFound;
        }

        account.IsVerified = false;

        await ResendOTP(dto);

        await _context.SaveChangesAsync();

        return ResponseStatus.Success;
    }

    public async Task<ResponseStatus> ResetPassword(ResetPasswordDTO dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == dto.email);

        if(account == null)
        {
            return ResponseStatus.NotFound;
        }
        if(!account.IsVerified)
        {
            return ResponseStatus.NotVerified;
        }

        var passwordData = Utils.HashPasswordWithSalt(dto.newPassword);
        // Hash the new password
        byte[] salt = passwordData.Salt;
        byte[] hashedPassword = passwordData.Hash;

        account.Salt = salt;
        account.Hashed = hashedPassword;

        await _context.SaveChangesAsync();

        return ResponseStatus.Success;
    }

    public async Task<ResponseStatus> RegisterAccount(RegisterAccountDTO account)
    {
        var accountID = await GenerateUniqueAccountId();
        var passwordData = Utils.HashPasswordWithSalt(account.password);
        var accountObject = new Account
        {
            AccountID = accountID,
            Email = account.email,
            Username = account.Username,
            Hashed = passwordData.Hash,
            Salt = passwordData.Salt,
            IsVerified = false,
            CreatedAt = DateTime.Now,
            OTP = null,
            LastTimeRequested = null,
            ExpiryAt = null,
            Image = await Utils.ConvertToByteArray(account.Image),
            ContentType = account.Image.ContentType
        };

        _context.Accounts.Add(accountObject);

        await _context.SaveChangesAsync();

        try
        {
            await SendOtpAsync(accountObject);

        } catch(Exception ex)
        {
            Console.WriteLine(ex.Message);

            return ResponseStatus.OTPNotSent;
        }
        
        return ResponseStatus.Success;
    }

    public async Task<LoginResult> Login(LoginDTO dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(x => x.Email == dto.email);

        if (account == null)
        {
            return new LoginResult
            {
                status = ResponseStatus.NotFound
            };
        }

        if (!account.IsVerified)
        {
            return new LoginResult
            {
                status = ResponseStatus.NotVerified
            };
        }

        var passwordData = Utils.HashPasswordWithSalt(dto.password);

        bool isValid = Utils.VerifyPassword(
            dto.password,
            account.Hashed,
            account.Salt);

        if (!isValid)
            return new LoginResult
            {
                status = ResponseStatus.InvalidPassword
            };

        var token = _jwtService.GenerateToken(account);
        
        return new LoginResult
        {
            status = ResponseStatus.Success,
            token = token
        };
    }
}