namespace Derivco.PlayerApi.Models;

using System.ComponentModel.DataAnnotations;

public class CreatePlayerRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Initial balance must be >= 0")]
    public decimal InitialBalance { get; set; }

    [Required]
    [StringLength(10)]
    public string Region { get; set; } = string.Empty;
}
