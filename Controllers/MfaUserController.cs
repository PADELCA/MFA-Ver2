using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MfaUserController : ControllerBase
    {
        private readonly MfaUserService _mfaUserService;

        public MfaUserController(MfaUserService mfaUserService)
        {
            _mfaUserService = mfaUserService;
        }

        [HttpPost("otp/{niklog}")]
        public async Task<IActionResult> GetByNiklog(string niklog)
        {
            if (string.IsNullOrWhiteSpace(niklog))
            {
                return BadRequest(new
                {
                    message = "Niklog wajib diisi."
                });
            }

            var user = await _mfaUserService.GetByNiklogAsync(niklog);

            if (user == null)
            {
                return NotFound(new
                {
                    found = false,
                    message = "Niklog tidak ditemukan."
                });
            }


            var newOtp = Random.Shared.Next(100000, 999999).ToString();


            var hashedOtp = BCrypt.Net.BCrypt.HashPassword(newOtp);

            await _mfaUserService.UpdateOtpHashAsync(user.Niklog, hashedOtp, DateTime.Now.AddHours(1)); // OTP berlaku 5 menit


            return Ok(new
            {
                id = user.Id,
                niklog = user.Niklog,
                message = "User ditemukan dan OTP berhasil dikirim.",
                encryptedOtp = hashedOtp 
            });
        }
    }
}