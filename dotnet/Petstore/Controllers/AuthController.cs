using Microsoft.AspNetCore.Mvc;
using Petstore.Accounts;
using Petstore.Models;

namespace Petstore.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher,
    JwtTokenService tokenService) : ControllerBase
{
    [HttpPost("signin")]
    [ProducesResponseType<SignInResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SignInResponseDto>> SignInAsync(
        SignInRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrEmpty(request.Password))
        {
            return InvalidCredentials();
        }

        var user = await accountRepository.FindUserAsync(request.UserId, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return InvalidCredentials();
        }

        var (token, expiresAt) = tokenService.IssueToken(user.UserId, user.Role);

        return Ok(new SignInResponseDto(token, user.UserId, user.Role, expiresAt));
    }

    private ObjectResult InvalidCredentials()
    {
        // Identical response for unknown user and wrong password (FR-008).
        return Unauthorized(new ApiErrorDto("auth.invalid_credentials", "User id or password is incorrect."));
    }
}
