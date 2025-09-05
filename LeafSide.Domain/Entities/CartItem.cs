using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeafSide.Domain.Entities;

public class CartItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    [Required]
    public Guid BookId { get; set; }
    public Book? Book { get; set; }
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    public decimal? PriceSnapshot { get; set; }
}


