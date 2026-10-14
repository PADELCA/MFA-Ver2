using System;

namespace Backend.Models
{
    public class MfaUser
    {
        public Guid Id { get; set; }
        public string Niklog { get; set; } = string.Empty;
    }
}