using System;

namespace Backend.Models
{
    public class MfaUser
    {
        public Guid Id { get; set; }
        public string Niklog { get; set; } = string.Empty;
    }
}
public class MfaUserOtpValidationDto
    {
        public Guid Id { get; set; }
        public string Niklog { get; set; } = string.Empty;
        public string? OtpHash { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class VerifyOtpRequest
    {
        public string OtpInput { get; set; } = string.Empty;
    }