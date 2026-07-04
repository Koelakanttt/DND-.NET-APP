namespace TableTop.Api.Contracts;

public record CreateRoomRequest(string Name);

public record RoomResponse(Guid Id, string Name, string JoinCode, DateTimeOffset CreatedAt);