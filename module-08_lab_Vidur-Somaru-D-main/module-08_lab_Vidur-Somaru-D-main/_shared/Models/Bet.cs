namespace Derivco.Shared.Models;

public class Bet
{
    public int BetId { get; set; }
    public int PlayerId { get; set; }
    public decimal Stake { get; set; }
    public decimal Payout { get; set; }
    public bool IsWin => Payout > Stake;
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}
