namespace Petstore.Models;

public sealed record RegisterRequestDto(
    string? UserId,
    string? Password,
    ContactInfoDto? Contact);
