using Microsoft.AspNetCore.Mvc;
using TableTop.Api.Contracts;
using TableTop.Core.Services;

namespace TableTop.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(RoomService roomService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RoomResponse>> Create(CreateRoomRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Название комнаты не может быть пустым.");

        var room = await roomService.CreateAsync(request.Name.Trim(), ct);
        var response = new RoomResponse(room.Id, room.Name, room.JoinCode, room.CreatedAt);
        return CreatedAtAction(nameof(GetByCode), new { joinCode = room.JoinCode }, response);
    }

    [HttpGet]
    public async Task<List<RoomResponse>> GetAll(CancellationToken ct)
    {
        var rooms = await roomService.GetAllAsync(ct);
        return rooms.Select(r => new RoomResponse(r.Id, r.Name, r.JoinCode, r.CreatedAt)).ToList();
    }

    [HttpGet("{joinCode}")]
    public async Task<ActionResult<RoomResponse>> GetByCode(string joinCode, CancellationToken ct)
    {
        var room = await roomService.FindByJoinCodeAsync(joinCode, ct);
        if (room is null)
            return NotFound();

        return new RoomResponse(room.Id, room.Name, room.JoinCode, room.CreatedAt);
    }
}