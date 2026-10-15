using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// проверки подлинности
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            // секретный ключ для подписи
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKeyForMyCrossPlatformProject2026!"))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// сначала Authentication, потом Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();