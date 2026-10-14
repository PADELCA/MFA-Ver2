using Backend.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Backend.Services
{
    public class MfaUserService
    {
        private readonly IConfiguration _configuration;

        public MfaUserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<MfaUser?> GetByNiklogAsync(string niklog)
        {

            string? connectionString = _configuration.GetConnectionString("MfaConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string belum dikonfigurasi.");
            }

            using var connection = new SqlConnection(connectionString);

            const string query = @"
                SELECT TOP 1 Id, Niklog
                FROM dbo.userr
                WHERE Niklog = @Niklog;
            ";

            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@Niklog", SqlDbType.NVarChar, 100)
                .Value = niklog;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new MfaUser
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Niklog = reader.GetString(reader.GetOrdinal("Niklog"))
            };
        }


        public async Task<bool> UpdateOtpHashAsync(string niklog, string hashedOtp, DateTime expiresAt)
        {
            string? connectionString = _configuration.GetConnectionString("MfaConnection");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string query = @"
        UPDATE dbo.userr 
        SET OtpHash = @OtpHash, 
            ExpiresAt = @ExpiresAt 
        WHERE Niklog = @Niklog;
    ";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@OtpHash", hashedOtp);
            command.Parameters.AddWithValue("@ExpiresAt", expiresAt);
            command.Parameters.AddWithValue("@Niklog", niklog);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

public async Task<MfaUserOtpValidationDto?> GetUserByValidOtpAsync(string otpInput)
{
    string? connectionString = _configuration.GetConnectionString("MfaConnection");

    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    const string query = @"
        SELECT Id, Niklog, OtpHash, ExpiresAt
        FROM dbo.userr
        WHERE ExpiresAt >= GETDATE() AND OtpHash IS NOT NULL;
    ";

    using var command = new SqlCommand(query, connection);
    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        var otpHash = reader.GetString(reader.GetOrdinal("OtpHash"));
        
        bool isValid = BCrypt.Net.BCrypt.Verify(otpInput, otpHash);
        if (isValid)
        {
            return new MfaUserOtpValidationDto
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Niklog = reader.GetString(reader.GetOrdinal("Niklog")),
                OtpHash = otpHash,
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt"))
            };
        }
    }

    return null; 
}
    }
}