using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrdering.Models;

public class MenuItem
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; }

    public virtual Category Category { get; set; } = null!; // Navigation properties
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

// Column	        Type
// Id (PK)	        int
// CategoryId (FK)	int
// Name	            varchar(150)
// Description	    varchar(255), nullable
// Price	        decimal(18,2)
// IsAvailable	    bool