using System.ComponentModel.DataAnnotations;

namespace RestaurantOrdering.API.Models;

public class Category{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}

// Column	    Type
// Id (PK)	    int
// Name	        varchar(100)
// Description	varchar(255), nullable