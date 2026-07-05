namespace TableTop.Core.Entities;

public class MapToken
{
    public Guid Id { get; set; }
    public string? ImageUrl { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#7c5cff";
    public double X { get; set; }   // позиция в ПРОЦЕНТАХ от ширины карты (0–100)
    public double Y { get; set; }   // в процентах от высоты
}