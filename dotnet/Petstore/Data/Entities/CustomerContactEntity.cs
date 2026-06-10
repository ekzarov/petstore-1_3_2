namespace Petstore.Data.Entities;

public sealed class CustomerContactEntity
{
    public required string UserId { get; set; }

    public required string FamilyName { get; set; }

    public required string GivenName { get; set; }

    public required string Street1 { get; set; }

    public string? Street2 { get; set; }

    public required string City { get; set; }

    public required string State { get; set; }

    public required string Zip { get; set; }

    public required string Country { get; set; }

    public required string Email { get; set; }

    public required string Phone { get; set; }

    public UserEntity? User { get; set; }
}
