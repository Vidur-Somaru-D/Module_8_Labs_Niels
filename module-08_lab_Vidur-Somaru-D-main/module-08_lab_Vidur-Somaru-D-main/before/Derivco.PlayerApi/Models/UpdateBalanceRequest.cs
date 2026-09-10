namespace Derivco.PlayerApi.Models;

using System.ComponentModel.DataAnnotations;

public class UpdateBalanceRequest
{
    [Range(0, double.MaxValue, ErrorMessage = "Balance must be >= 0")]
    public decimal NewBalance { get; set; }
}
