namespace Derivco.PlayerApi.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    public int PlayerId { get; set; }
    public decimal Amount { get; set; }  // positive = credit, negative = debit
    public string TransactionType { get; set; } = string.Empty;  // "Transfer", "Deposit", "Withdrawal"
    public DateTime CreatedAt { get; set; }
}
