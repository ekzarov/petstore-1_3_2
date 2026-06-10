namespace Petstore.Models;

public sealed record AccountDto(
    string UserId,
    ContactInfoDto? Contact);
