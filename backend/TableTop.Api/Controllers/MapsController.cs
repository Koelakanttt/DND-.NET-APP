using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableTop.Infrastructure.Data;

namespace TableTop.Api.Controllers;

[ApiController]
[Route("api/rooms/{joinCode}/map")]
public class MapsController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] AllowedTypes = ["image/png", "image/jpeg", "image/webp"];
    private const long MaxSize = 20 * 1024 * 1024; // 20 МБ

    [HttpPost]
    [RequestSizeLimit(MaxSize)]
    public async Task<IActionResult> Upload(string joinCode, IFormFile file, CancellationToken ct)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.JoinCode == joinCode, ct);
        if (room is null) return NotFound();

        if (file.Length == 0 || file.Length > MaxSize)
            return BadRequest("Файл пустой или больше 20 МБ.");
        if (!AllowedTypes.Contains(file.ContentType))
            return BadRequest("Только PNG, JPEG или WebP.");

        var ext = file.ContentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        var mapsDir = Path.Combine(env.ContentRootPath, "uploads", "maps");
        Directory.CreateDirectory(mapsDir);

        var fileName = $"{room.Id}{ext}";
        var fullPath = Path.Combine(mapsDir, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
            await file.CopyToAsync(stream, ct);

        room.MapUrl = $"/uploads/maps/{fileName}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        await db.SaveChangesAsync(ct);

        return Ok(new { mapUrl = room.MapUrl });
    }
}