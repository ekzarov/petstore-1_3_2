namespace Petstore.Models;

public sealed record PlaceOrderRequestDto(
    ContactInfoDto? ShippingContact,
    ContactInfoDto? BillingContact);
