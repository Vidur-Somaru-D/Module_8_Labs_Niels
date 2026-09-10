namespace Derivco.Shared.Models;

public class Player
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public string Region { get; set; } = "EU";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Transaction> RecentTransactions { get; set; } = [];
    public List<Bet> RecentBets { get; set; } = [];
}
