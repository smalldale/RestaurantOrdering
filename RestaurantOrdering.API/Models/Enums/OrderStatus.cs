namespace RestaurantOrdering.API.Models.Enums;

/// <summary>
/// Lifecycle status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been created and can still be edited.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Order has been checked out or explicitly started and is being prepared.
    /// </summary>
    Preparing = 1,

    /// <summary>
    /// Order is prepared and waiting to be completed or handed to the customer.
    /// </summary>
    Ready = 2,

    /// <summary>
    /// Order is finished. This is a terminal status.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Order was canceled. This is a terminal status.
    /// </summary>
    Canceled = 4
}
