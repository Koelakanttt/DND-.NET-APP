namespace TableTop.Core.Entities;

public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public List<ChatMessage> Messages { get; set; } = [];
    public string? MapUrl { get; set; }
}