using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialAggregatorAPI.Models
{
    public class NewsArticle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // Auto-incremented ID
        
        [Required]
        public string? Title { get; set; }  // News title

        [Required]
        public string? Content { get; set; }  // Full news content

        public string? Source { get; set; }  // Where the news came from (e.g., API name)

        public DateTime PublishedAt { get; set; }  // When the news was published

        public bool IsProcessed { get; set; } = false;  // If the AI has generated a post from it
    }
}
