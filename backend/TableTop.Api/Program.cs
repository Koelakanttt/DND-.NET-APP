using Microsoft.EntityFrameworkCore;
using TableTop.Core.Interfaces;
using TableTop.Core.Services;
using TableTop.Infrastructure.Data;
using TableTop.Infrastructure.Repositories;
using TableTop.Api.Hubs;
using Microsoft.Extensions.FileProviders;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<DiceService>();
builder.Services.AddSignalR();  
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
var app = builder.Build();
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});
app.MapGet("/ping", () => "pong");
app.MapHub<GameHub>("/hub/game"); 
app.MapControllers();

app.Run();