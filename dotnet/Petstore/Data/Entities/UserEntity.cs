namespace Petstore.Data.Entities;

public sealed class UserEntity
{
    public required string UserId { get; set; }

    public required byte[] PasswordHash { get; set; }

    public required byte[] PasswordSalt { get; set; }

    public required string Role { get; set; }

    public required DateTime CreatedAt { get; set; }

    public CustomerContactEntity? Contact { get; set; }
}
