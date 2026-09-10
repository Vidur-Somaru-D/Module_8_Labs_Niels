namespace Derivco.PlayerApi.Services;

using Derivco.PlayerApi.Models;

public interface IPlayerService
{
    PlayerDto? GetPlayer(int id);
    PlayerDto CreatePlayer(CreatePlayerRequest request);
    PlayerDto? UpdateBalance(int playerId, decimal newBalance);
    IEnumerable<PlayerDto> GetAllPlayers();
    void TransferFunds(TransferFundsRequest request);
}
