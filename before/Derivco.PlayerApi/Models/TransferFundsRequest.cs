namespace Derivco.PlayerApi.Models;

using System.ComponentModel.DataAnnotations;

public class TransferFundsRequest
{
    [Required]
    public int FromPlayerId { get; set; }
    [Required]
    public int ToPlayerId { get; set; }
    [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than 0")]
    public decimal Amount { get; set; }
}
