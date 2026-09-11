namespace Derivco.PlayerApi.Services;

using System.Collections.Generic;
using Derivco.PlayerApi.Models;

public interface IPlayerService
{
    PlayerDto? GetPlayer(int id);
    PlayerDto CreatePlayer(CreatePlayerRequest request);
    PlayerDto? UpdateBalance(int playerId, decimal newBalance);
    IEnumerable<PlayerDto> GetAllPlayers();
    IReadOnlyCollection<TransactionDto>? GetPlayerTransactions(int playerId);
    void TransferFunds(TransferFundsRequest request);
}
