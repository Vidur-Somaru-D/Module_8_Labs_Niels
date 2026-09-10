namespace Derivco.PlayerApi.Repositories;

using Derivco.PlayerApi.Models;

public interface IPlayerRepository
{
    Player? GetById(int id);
    Player Create(Player player);
    Player? UpdateBalance(int playerId, decimal newBalance);
    IEnumerable<Player> GetAll();
    IEnumerable<Transaction> GetTransactionsByPlayer(int playerId);
    void TransferFunds(int fromPlayerId, int toPlayerId, decimal amount);
}
