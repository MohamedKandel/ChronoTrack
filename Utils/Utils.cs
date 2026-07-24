using System.Security.Cryptography;
using System.Text;

public class Utils
{
    public static string GenerateOTP()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public static string GenerateOtpEmailTemplate(string otp, string platformName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>OTP Verification</title>
</head>

<body style='margin:0; padding:0; font-family:Arial, sans-serif; background-color:#f4f4f4;'>

    <table width='100%' cellpadding='0' cellspacing='0' style='padding:20px 0;'>
        <tr>
            <td align='center'>

                <table width='500' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:10px; overflow:hidden; box-shadow:0 2px 10px rgba(0,0,0,0.1);'>

                    <!-- Header -->
                    <tr>
                        <td style='background:#4F46E5; padding:20px; text-align:center; color:#ffffff; font-size:20px; font-weight:bold;'>
                            {platformName} Platform
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style='padding:30px; text-align:center;'>

                            <h2 style='color:#333;'>Email Verification</h2>

                            <p style='color:#666; font-size:14px;'>
                                Use the OTP below to verify your account. This code is valid for <b>5 minutes</b>.
                            </p>

                            <div style='margin:25px 0; font-size:28px; letter-spacing:6px; font-weight:bold; color:#4F46E5;'>
                                {otp}
                            </div>

                            <p style='color:#999; font-size:12px;'>
                                If you did not request this, please ignore this email.
                            </p>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background:#f0f0f0; padding:15px; text-align:center; font-size:12px; color:#777;'>
                            © {DateTime.Now.Year} {platformName} Platform. All rights reserved.
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";
    }

    public static string GenerateId() => Guid.NewGuid().ToString("N");

    public static (byte[] Hash, byte[] Salt) HashPasswordWithSalt(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return (hash, salt);
    }

    public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);

        var computedHash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(password));

        return CryptographicOperations.FixedTimeEquals(
            computedHash,
            storedHash);
    }

    public static async Task<byte[]> ConvertToByteArray(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

}