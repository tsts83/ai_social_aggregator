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

        public string? Url { get; set; }  // Link to the news article

        public DateTime PublishedAt { get; set; }  // When the news was published

        public bool IsProcessed { get; set; } = false;  // If the AI has generated a post from it
        
        public bool IsPosted { get; set; } = false;  // If the article has been posted  

        public string? ThumbnailUrl { get; set; }  // URL of the thumbnail image

        public string? AiSummary { get; set; }  // AI-generated summary  
    }
}
