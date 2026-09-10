namespace Derivco.PlayerApi.Repositories;

using Derivco.PlayerApi.Models;

// Retained for reference only — not registered in Program.cs.
// All endpoints in the after/ solution run against SqlPlayerRepository.
public class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly Dictionary<int, Player> _players = new();
    private int _nextId = 1;

    public InMemoryPlayerRepository()
    {
        var seed = new[]
        {
            new Player { PlayerId = _nextId++, Username = "player_za_001", Balance = 500.00m,  IsActive = true,  Region = "ZA" },
            new Player { PlayerId = _nextId++, Username = "player_uk_001", Balance = 1200.50m, IsActive = true,  Region = "UK" },
            new Player { PlayerId = _nextId++, Username = "player_eu_001", Balance = 0.00m,    IsActive = false, Region = "EU" },
        };
        foreach (var p in seed)
            _players[p.PlayerId] = p;
    }

    public Player? GetById(int id) => _players.GetValueOrDefault(id);

    public Player Create(Player player)
    {
        player.PlayerId = _nextId++;
        player.IsActive = true;
        _players[player.PlayerId] = player;
        return player;
    }

    public Player? UpdateBalance(int playerId, decimal newBalance)
    {
        if (!_players.TryGetValue(playerId, out var player))
            return null;
        player.Balance = newBalance;
        return player;
    }

    public IEnumerable<Player> GetAll() => _players.Values;

    public IEnumerable<Transaction> GetTransactionsByPlayer(int playerId) =>
        throw new NotImplementedException("InMemoryPlayerRepository does not support transactions.");

    public void TransferFunds(int fromPlayerId, int toPlayerId, decimal amount) =>
        throw new NotImplementedException("InMemoryPlayerRepository does not support TransferFunds.");
}
