using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOrdering.Models.Enums;

namespace RestaurantOrdering.Models;

public class Order
{
    public int Id { get; set; }

    [MaxLength(30)]
    public string OrderNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CustomerName { get; set; }

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }
    
    public DateTime? PaymentDate { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
}

// Column       Type
// Id (PK)	    int
// OrderNumber	varchar(30)
// CustomerName	varchar(100), nullable
// OrderDate	datetime
// Status	    int (enum)
// TotalAmount	decimal(18,2)