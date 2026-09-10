namespace Derivco.Shared.Interfaces;

using Derivco.Shared.Models;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(int playerId);
    Task<IEnumerable<Player>> GetAllAsync();
    Task SaveAsync(Player player);
    Task DeleteAsync(int playerId);
}
