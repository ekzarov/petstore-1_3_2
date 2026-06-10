namespace Petstore.Models;

public sealed record SignInResponseDto(
    string Token,
    string UserId,
    string Role,
    DateTime ExpiresAt);
