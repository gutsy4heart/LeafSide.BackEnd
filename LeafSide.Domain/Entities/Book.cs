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
    
    public string Isbn { get; set; } = string.Empty;
    public string Language { get; set; } = "Russian";
    public int PageCount { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
