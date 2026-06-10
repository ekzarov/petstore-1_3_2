using Petstore.Models;

namespace Petstore.Accounts;

public static class ContactValidation
{
    public static IReadOnlyList<string> MissingContactFields(ContactInfoDto? contact)
    {
        if (contact is null)
        {
            return ["contact"];
        }

        var missing = new List<string>();
        AddIfMissing(missing, contact.FamilyName, "contact.familyName");
        AddIfMissing(missing, contact.GivenName, "contact.givenName");
        AddIfMissing(missing, contact.Street1, "contact.street1");
        AddIfMissing(missing, contact.City, "contact.city");
        AddIfMissing(missing, contact.State, "contact.state");
        AddIfMissing(missing, contact.Zip, "contact.zip");
        AddIfMissing(missing, contact.Country, "contact.country");
        AddIfMissing(missing, contact.Email, "contact.email");
        AddIfMissing(missing, contact.Phone, "contact.phone");

        return missing;
    }

    private static void AddIfMissing(List<string> missing, string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            missing.Add(field);
        }
    }
}
