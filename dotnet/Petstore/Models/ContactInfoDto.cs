namespace Petstore.Models;

public sealed record ContactInfoDto(
    string FamilyName,
    string GivenName,
    string Street1,
    string? Street2,
    string City,
    string State,
    string Zip,
    string Country,
    string Email,
    string Phone);
