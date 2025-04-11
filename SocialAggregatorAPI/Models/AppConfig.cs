using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SocialAggregatorAPI.Models;

public class AppConfig
{   
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string? Section { get; set; }

    public string? Key { get; set; }

    [Required]
    public string? Value { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static implicit operator AppConfig(AppSettings v)
    {
        throw new NotImplementedException();
    }
}
