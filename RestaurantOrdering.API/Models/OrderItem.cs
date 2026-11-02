using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrdering.API.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int MenuItemId { get; set; }
    
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    public virtual Order Order { get; set; } = null!;

    public MenuItem MenuItem { get; set; } = null!;
}


// Column	        Type
// Id (PK)	        int
// OrderId (FK)	    int
// MenuItemId (FK)	int
// Quantity	        int
// UnitPrice	    decimal(18,2)
// Subtotal	        decimal(18,2)