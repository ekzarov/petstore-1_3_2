namespace Petstore.Data;

public static class AccountModelConstants
{
    public static class Tables
    {
        public const string Users = "Users";
        public const string CustomerContacts = "CustomerContacts";
    }

    public static class Lengths
    {
        public const int UserId = 64;
        public const int Role = 16;
        public const int Name = 128;
        public const int Street = 128;
        public const int City = 64;
        public const int State = 64;
        public const int Zip = 16;
        public const int Country = 64;
        public const int Email = 256;
        public const int Phone = 32;
        public const int PasswordHash = 64;
        public const int PasswordSalt = 16;
    }

    public static class Roles
    {
        public const string Customer = "customer";
        public const string Admin = "admin";
    }
}
