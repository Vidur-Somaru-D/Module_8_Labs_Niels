namespace Derivco.PlayerApi.Models;

public class Player
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public string Region { get; set; } = string.Empty;
}
