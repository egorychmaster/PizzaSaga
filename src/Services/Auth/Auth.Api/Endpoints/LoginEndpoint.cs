using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Endpoints;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", HandleAsync)
           .WithName("Login")
           .WithTags("Auth")
           .Produces<LoginResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status401Unauthorized);
    }

    private static IResult HandleAsync(LoginRequest request, IConfiguration config)
    {
        var testUserSection = config.GetSection("TestUser");
        var configuredUsername = testUserSection["Username"];
        var configuredPasswordHash = testUserSection["PasswordHash"];
        var userId = testUserSection["UserId"];

        // 1. Валидация входных данных
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { error = "Email and password are required." });
        }

        // 2. Проверка учетных данных
        bool isValidUser = string.Equals(request.Email, configuredUsername, StringComparison.OrdinalIgnoreCase)
                           && BCrypt.Net.BCrypt.Verify(request.Password, configuredPasswordHash);

        if (!isValidUser)
        {
            return Results.Unauthorized();
        }

        // 3. Генерация Access Token & Refresh Token
        var jwtSettings = config.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");
        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId!),
            new Claim(JwtRegisteredClaimNames.Email, request.Email),
            new Claim("role", "admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            //expires: expires,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new LoginResponse(
            AccessToken: accessToken,
            TokenType: "Bearer"
        ));
    }
}

// DTO контракты (согласно api.md / v1)
public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string TokenType);
