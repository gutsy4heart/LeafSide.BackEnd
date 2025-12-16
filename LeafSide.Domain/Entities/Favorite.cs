using System.ComponentModel.DataAnnotations;

namespace LeafSide.Domain.Entities;

public class Favorite
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid BookId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Book? Book { get; set; }
}

