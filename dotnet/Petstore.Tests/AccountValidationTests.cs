using Petstore.Accounts;
using Petstore.Models;

namespace Petstore.Tests;

public sealed class AccountValidationTests
{
    private static readonly ContactInfoDto ValidContact = new(
        "Doe", "Jane", "1 Main Street", null, "Springfield", "IL", "62701", "USA", "jane@example.com", "555-0100");

    [Fact]
    public void MissingContactFields_Returns_Contact_When_Null()
    {
        var result = ContactValidation.MissingContactFields(null);
        var field = Assert.Single(result);
        Assert.Equal("contact", field);
    }

    [Fact]
    public void MissingContactFields_Returns_Empty_When_All_Fields_Valid()
    {
        var result = ContactValidation.MissingContactFields(ValidContact);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null, "contact.familyName")]
    [InlineData("", "contact.familyName")]
    [InlineData("   ", "contact.familyName")]
    public void MissingContactFields_Validates_FamilyName(string? value, string expectedField)
    {
        var contact = ValidContact with { FamilyName = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Theory]
    [InlineData(null, "contact.givenName")]
    [InlineData("", "contact.givenName")]
    [InlineData("   ", "contact.givenName")]
    public void MissingContactFields_Validates_GivenName(string? value, string expectedField)
    {
        var contact = ValidContact with { GivenName = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Theory]
    [InlineData(null, "contact.street1")]
    [InlineData("", "contact.street1")]
    [InlineData("   ", "contact.street1")]
    public void MissingContactFields_Validates_Street1(string? value, string expectedField)
    {
        var contact = ValidContact with { Street1 = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Theory]
    [InlineData(null, "contact.city")]
    [InlineData("", "contact.city")]
    [InlineData("   ", "contact.city")]
    public void MissingContactFields_Validates_City(string? value, string expectedField)
    {
        var contact = ValidContact with { City = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Theory]
    [InlineData(null, "contact.state")]
    [InlineData("", "contact.state")]
    [InlineData("   ", "contact.state")]
    public void MissingContactFields_Validates_State(string? value, string expectedField)
    {
        var contact = ValidContact with { State = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Theory]
    [InlineData(null, "contact.zip")]
    [InlineData("", "contact.zip")]
    [InlineData("   ", "contact.zip")]
    public void MissingContactFields_Validates_Zip(string? value, string expectedField)
    {
        var contact = ValidContact with { Zip = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Theory]
    [InlineData(null, "contact.country")]
    [InlineData("", "contact.country")]
    [InlineData("   ", "contact.country")]
    public void MissingContactFields_Validates_Country(string? value, string expectedField)
    {
        var contact = ValidContact with { Country = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Theory]
    [InlineData(null, "contact.email")]
    [InlineData("", "contact.email")]
    [InlineData("   ", "contact.email")]
    public void MissingContactFields_Validates_Email(string? value, string expectedField)
    {
        var contact = ValidContact with { Email = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Theory]
    [InlineData(null, "contact.phone")]
    [InlineData("", "contact.phone")]
    [InlineData("   ", "contact.phone")]
    public void MissingContactFields_Validates_Phone(string? value, string expectedField)
    {
        var contact = ValidContact with { Phone = value! };
        var result = ContactValidation.MissingContactFields(contact);
        Assert.Contains(expectedField, result);
    }

    [Fact]
    public void MissingContactFields_Street2_Is_Optional_And_Allowed_Null_Or_Empty()
    {
        var contactNull = ValidContact with { Street2 = null };
        var resultNull = ContactValidation.MissingContactFields(contactNull);
        Assert.Empty(resultNull);

        var contactEmpty = ValidContact with { Street2 = "" };
        var resultEmpty = ContactValidation.MissingContactFields(contactEmpty);
        Assert.Empty(resultEmpty);
    }
}
