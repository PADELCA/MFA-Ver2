using Backend.Services;
using Microsoft.OpenApi.Models; // 1. Pastikan using ini ada di paling atas

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. TAMBAHKAN BAGIAN INI SECARA LENGKAP
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backend v1",
        Version = "v1"
    });
});

builder.Services.AddScoped<MfaUserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = true; // Tambahkan baris ini
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();