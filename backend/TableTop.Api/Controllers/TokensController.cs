using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TableTop.Api.Hubs;
using TableTop.Infrastructure.Data;

namespace TableTop.Api.Controllers;

[ApiController]
[Route("api/rooms/{joinCode}/tokens")]
public class TokensController(AppDbContext db, IWebHostEnvironment env, IHubContext<GameHub> hub) : ControllerBase
{
    private static readonly string[] AllowedTypes = ["image/png", "image/jpeg", "image/webp"];
    private const long MaxSize = 5 * 1024 * 1024;

    [HttpPost("{tokenId:guid}/image")]
    [RequestSizeLimit(MaxSize)]
    public async Task<IActionResult> UploadImage(string joinCode, Guid tokenId, IFormFile file, CancellationToken ct)
    {
        var token = await db.MapTokens
            .Include(t => t.Room)
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.Room!.JoinCode == joinCode, ct);
        if (token is null) return NotFound();

        if (file.Length == 0 || file.Length > MaxSize) return BadRequest("Файл пустой или больше 5 МБ.");
        if (!AllowedTypes.Contains(file.ContentType)) return BadRequest("Только PNG, JPEG или WebP.");

        var ext = file.ContentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        var dir = Path.Combine(env.ContentRootPath, "uploads", "tokens");
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, $"{token.Id}{ext}");
        await using (var stream = System.IO.File.Create(fullPath))
            await file.CopyToAsync(stream, ct);

        token.ImageUrl = $"/uploads/tokens/{token.Id}{ext}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        await db.SaveChangesAsync(ct);

        await hub.Clients.Group(joinCode).SendAsync("TokenImageSet", token.Id, token.ImageUrl, ct);
        return Ok(new { imageUrl = token.ImageUrl });
    }
}