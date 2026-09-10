namespace Derivco.Shared.Models;

// NOTE: In Module 1 this struct is deliberately made too large (a teaching example).
// The correct version here has only value-type fields.
public readonly struct PlayerEvent
{
    public int PlayerId { get; }
    public int EventCode { get; }
    public DateTime Timestamp { get; }

    public PlayerEvent(int playerId, int eventCode, DateTime timestamp)
    {
        PlayerId = playerId;
        EventCode = eventCode;
        Timestamp = timestamp;
    }
}
