namespace Derivco.PlayerApi.Services;

using Derivco.PlayerApi.Models;
using Derivco.PlayerApi.Repositories;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _repository;

    public PlayerService(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public PlayerDto? GetPlayer(int id)
    {
        var player = _repository.GetById(id);
        if (player is null) return null;
        return MapToDto(player);
    }

    public PlayerDto CreatePlayer(CreatePlayerRequest request)
    {
        var player = new Player
        {
            Username = request.Username,
            Balance = request.InitialBalance,
            Region = request.Region,
        };
        var created = _repository.Create(player);
        return MapToDto(created);
    }

    public PlayerDto? UpdateBalance(int playerId, decimal newBalance)
    {
        var updated = _repository.UpdateBalance(playerId, newBalance);
        if (updated is null) return null;
        return MapToDto(updated);
    }

    public IEnumerable<PlayerDto> GetAllPlayers()
    {
        return _repository.GetAll().Select(MapToDto);
    }

    public void TransferFunds(TransferFundsRequest request)
    {
        // Pre-flight existence checks give friendly 404s before touching the transaction.
        // The actual balance check inside TransferFunds uses UPDLOCK to avoid TOCTOU races.
        var from = _repository.GetById(request.FromPlayerId)
            ?? throw new KeyNotFoundException($"Player {request.FromPlayerId} not found");
        var to = _repository.GetById(request.ToPlayerId)
            ?? throw new KeyNotFoundException($"Player {request.ToPlayerId} not found");

        _ = from;
        _ = to;

        _repository.TransferFunds(request.FromPlayerId, request.ToPlayerId, request.Amount);
    }

    private static PlayerDto MapToDto(Player player) => new()
    {
        PlayerId = player.PlayerId,
        Username = player.Username,
        Balance = player.Balance,
        Region = player.Region,
    };
}
