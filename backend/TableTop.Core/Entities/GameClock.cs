namespace TableTop.Core.Entities;

public enum ClockMode { Normal, Combat }

public class GameClock
{
    public Guid RoomId { get; set; }         
    public Room? Room { get; set; }
    public long WorldMinutes { get; set; }    
    public ClockMode Mode { get; set; }
    public int CombatRound { get; set; } 
}