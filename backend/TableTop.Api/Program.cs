using Microsoft.EntityFrameworkCore;
using TableTop.Core.Interfaces;
using TableTop.Core.Services;
using TableTop.Infrastructure.Data;
using TableTop.Infrastructure.Repositories;
using TableTop.Api.Hubs;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<DiceService>();
builder.Services.AddSignalR();  
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<RoomService>();

var app = builder.Build();

app.MapGet("/ping", () => "pong");
app.MapHub<GameHub>("/hub/game"); 
app.MapControllers();

app.Run();