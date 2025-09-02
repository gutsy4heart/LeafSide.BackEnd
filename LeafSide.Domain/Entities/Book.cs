using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafSide.Domain.Entities;

public class Book
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Only letters.")]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public string Author { get; set; } = string.Empty;
    [Required]
    public string Genre { get; set; } = string.Empty;
    [Required]
    public string Publishing { get; set; } = string.Empty;
    [Required]
    public string Created { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    [Required]
    public decimal? Price { get; set; }
}
