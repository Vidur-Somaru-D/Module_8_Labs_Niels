namespace Derivco.Shared.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    public int PlayerId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "Debit" | "Credit"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
