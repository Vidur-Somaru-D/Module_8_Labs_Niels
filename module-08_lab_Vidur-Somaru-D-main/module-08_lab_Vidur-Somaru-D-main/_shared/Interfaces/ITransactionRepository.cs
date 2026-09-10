namespace Derivco.Shared.Interfaces;

using Derivco.Shared.Models;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetByPlayerIdAsync(int playerId);
    Task AddAsync(Transaction transaction);
}
