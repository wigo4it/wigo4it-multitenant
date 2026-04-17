using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Wigo4it.MultiTenant.AspNetCore.Sample;

/// <summary>
/// Onveilige JWT validator uitsluitend voor voorbeelddoeleinden - valideert JWT zonder de handtekening te controleren.
/// NOOIT gebruiken in productie!
/// </summary>
internal class UnsafeJwtTokenHandler : TokenHandler
{
    private static readonly JwtSecurityTokenHandler JwtHandler = new();

    public override Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(
                new TokenValidationResult
                {
                    IsValid = false,
                    Exception = new SecurityTokenMalformedException("Bearer token cannot be empty."),
                }
            );
        }

        if (!JwtHandler.CanReadToken(token))
        {
            return Task.FromResult(
                new TokenValidationResult
                {
                    IsValid = false,
                    Exception = new SecurityTokenMalformedException("Bearer token must be valid."),
                }
            );
        }

        try
        {
            var jwtToken = JwtHandler.ReadJwtToken(token);

            var result = new TokenValidationResult
            {
                ClaimsIdentity = new ClaimsIdentity(jwtToken.Claims, JwtBearerDefaults.AuthenticationScheme),
                SecurityToken = jwtToken,
                IsValid = true,
            };
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TokenValidationResult { IsValid = false, Exception = ex });
        }
    }
}