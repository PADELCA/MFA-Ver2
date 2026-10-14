using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using QRCoder;
using Backend.Models;

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

            var expiresAt = DateTime.Now.AddHours(1);

            await _mfaUserService.UpdateOtpHashAsync(user.Niklog, hashedOtp, expiresAt);

            using var qrGenerator = new QRCodeGenerator();

            using var qrCodeData = qrGenerator.CreateQrCode(newOtp, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20); // Ukuran pixel QR

            string base64String = Convert.ToBase64String(qrCodeBytes);
            string qrCodeDataUrl = $"data:image/png;base64,{base64String}";

            return Ok(new
            {
                id = user.Id,
                niklog = user.Niklog,
                message = "User ditemukan dan OTP berhasil dikirim (berlaku 1 jam).",
                OtpLink = qrCodeDataUrl
            });
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OtpInput))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP wajib diisi."
                });
            }

            var userData = await _mfaUserService.GetUserByValidOtpAsync(request.OtpInput);

            if (userData == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP salah atau sudah kedaluwarsa."
                });
            }

            return Ok(new
            {
                message = "OTP benar! Verifikasi berhasil.",
                niklog = userData.Niklog
            });
        }
    }
}