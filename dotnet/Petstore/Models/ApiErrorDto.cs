namespace Petstore.Models;

public sealed record ApiErrorDto(
    string Code,
    string Message);
