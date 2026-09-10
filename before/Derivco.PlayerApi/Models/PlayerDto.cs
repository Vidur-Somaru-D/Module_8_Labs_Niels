namespace Derivco.PlayerApi.Models;

public class PlayerDto
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Region { get; set; } = string.Empty;
}
