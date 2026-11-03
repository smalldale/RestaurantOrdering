namespace RestaurantOrdering.API.Models.Enums;

/// <summary>
/// Payment method recorded when an order is checked out.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Cash payment.
    /// </summary>
    Cash = 0,

    /// <summary>
    /// Card payment.
    /// </summary>
    Card = 1,

    /// <summary>
    /// QRIS (QR code) payment.
    /// </summary>
    QRIS = 2
}
