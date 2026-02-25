using Shared;
using Shared.Data;
using Shared.Interfaces;
using Shared.Services;
using Web.Components;
using Web.Services;
using Microsoft.EntityFrameworkCore;
using Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<MyStarcraftBot>();
builder.Services.AddSingleton<StarCraftService>();
builder.Services.AddSingleton<UserPreferencesService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMatchRepository, MatchRepository>();
builder.Services.AddSingleton<IGameEventRepository, GameEventRepository>();

// add SignalR services
builder.Services.AddSignalR();
builder.Services.AddHostedService<GameWorker>();

// Configure EF Core with SQLite
var matchDbPath = Path.GetFullPath(
    Path.Combine(
        AppContext.BaseDirectory,
        "../../../../Shared/Data/matches.db"
    )
);
// Database path:
// C:\Users\jidapa.angsutti\coding\starcraft2-jidapa-solo\broodwarStarterWindows\Shared\Data\matches.db
Console.WriteLine($"Database path: {matchDbPath}");

// Ensure Data directory exists
Directory.CreateDirectory(Path.GetDirectoryName(matchDbPath)!);

builder.Services.AddDbContextFactory<MatchContext>(options =>
    options.UseSqlite($"Data Source={matchDbPath}"));


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MatchContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database initialized.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

// SignalR hub mapping
app.MapHub<GameStateHub>("api/bot/gameHub");

var starcraftService = app.Services.GetRequiredService<StarCraftService>();
var bot = app.Services.GetRequiredService<MyStarcraftBot>();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var _ = Task.Run(() =>
{
    app.Run();
});

bot.Connect();

starcraftService.StopAndReset();

public partial class Program { }
