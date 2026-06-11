namespace Petstore.Orders;

/// <summary>
/// Canonical legacy status vocabulary from the J2EE processmanager
/// (OrderStatusNames). Only PENDING is assigned by this slice; the rest
/// are reserved for order processing (010) and fulfillment (011).
/// </summary>
public static class OrderStatus
{
    public const string Pending = "PENDING";
    public const string Approved = "APPROVED";
    public const string Denied = "DENIED";
    public const string ShippedPart = "SHIPPED_PART";
    public const string Shipped = "SHIPPED";
    public const string Completed = "COMPLETED";

    public static readonly IReadOnlyList<string> All =
        [Pending, Approved, Denied, ShippedPart, Shipped, Completed];
}
