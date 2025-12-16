using System.ComponentModel.DataAnnotations;

namespace LeafSide.API.Requests.Reviews;

public class UpdateReviewRequest
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [MaxLength(2000)]
    public string? Comment { get; set; }
}

