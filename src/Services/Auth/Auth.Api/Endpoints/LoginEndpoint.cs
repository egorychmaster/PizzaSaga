using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        // Из конфига получаю пользователя
        var testUserSection = config.GetSection("TestUser");
        var configuredUsername = testUserSection["Username"];
        var configuredPasswordHash = testUserSection["PasswordHash"];
        var configuredUserId = testUserSection["UserId"];

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest(new { error = "Email and password are required." });

        bool isValidUser = string.Equals(request.Email, configuredUsername, StringComparison.OrdinalIgnoreCase)
            && BCrypt.Net.BCrypt.Verify(request.Password, configuredPasswordHash);
        if (!isValidUser)
            return Results.Unauthorized();

        // signingCredentials
        var jwtSettings = config.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // claims
        /*
         * утверждения (утверждаемые свойства) о токене, пользователе или контексте аутентификации.
         {
            "jti": "14fff3b3-17e8-4ff4-a299-16600c2c3275",
            "sub": "2fbb95d2-0672-4b2f-b3fd-447d2a02d5f8",
            "email": "admin@test.com",
            "role": "admin"              
         }
         */
        var claims = new[]
        {
            // jti — JWT ID Уникальный идентификатор конкретного экземпляра JWT.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // subject - кто является субъектом токена.
            // Это идентификатор пользователя, что означает: Этот JWT выдан пользователю с UserId = 2fbb95d2-0672-4b2f-b3fd-447d2a02d5f8.
            new Claim(JwtRegisteredClaimNames.Sub, configuredUserId!),
            
            // email Это обычный пользовательский claim, содержащий email: "email": "admin@test.com"
            // Он нужен, если downstream-сервису действительно нужен email пользователя.
            // Нужно ли тебе добавлять email? Не обязательно. Для Order Service в текущем сценарии достаточно: sub → CustomerId
            new Claim(JwtRegisteredClaimNames.Email, request.Email),
            
            // role - роль пользователя: "role": "admin" Используется для авторизации.
            // Например: [Authorize(Roles = "admin")] или: .RequireAuthorization(policy =>  policy.RequireRole("admin"));
            new Claim("role", "admin")
        };

        // expires
        var expiresMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");
        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var token = new JwtSecurityToken(
            signingCredentials: creds,
            claims: claims
            //expires: expires
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
