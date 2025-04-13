using System.Net;
using System.Net.WebSockets;
using System.Text;
using DAL;
using DAL.Context;
using Microsoft.EntityFrameworkCore;
using WebApp.Pages;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
connectionString = connectionString.Replace("<%DB_PATH%>", GameStorageDb.Instance.SavePath);
builder.Services.AddDbContext<UnoDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    // Configure session options here
    // options.Cookie.Name = "YourSessionCookieName";
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Set the session timeout duration
    // Other session configuration options...
});


var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

// Middleware to handle WebSocket requests
app.UseWebSockets(new WebSocketOptions
{
    // Configure options for WebSocket handling
    // For example, you can set the KeepAliveInterval, etc.
});

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/Game" && context.WebSockets.IsWebSocketRequest)
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        // Handle WebSocket connections here
        await HandleWebSocketConnections(context, webSocket);
    }
    else
    {
        await next();
    }
});

app.Run();
return;

async Task HandleWebSocketConnections(HttpContext context, WebSocket webSocket)
{
    var gameId = context.Request.Query["Id"].ToString();
    var playerName = context.Request.Query["Player"].ToString();
    Console.WriteLine(gameId);
    Console.WriteLine(playerName);

    if (!Guid.TryParse(gameId, out Guid guid)) return;

    // Check if the player can connect based on parameters
    bool canConnect;
    lock (Game.ConnectedUsers)
    {
        if (!Game.ConnectedUsers.ContainsKey(guid))
        {
            Game.ConnectedUsers[guid] = new Dictionary<string, WebSocket>();
        }

        canConnect = !Game.ConnectedUsers[guid].ContainsKey(playerName)
                     || Game.UsersInGracePeriod.ContainsKey(playerName);
    }

    Console.WriteLine("canConnect: " + canConnect);

    if (!canConnect) return;

    // Add or update the player in the game
    lock (Game.ConnectedUsers)
    {
        Game.ConnectedUsers[guid][playerName] = webSocket;
    }

    bool isNewPlayer;

    // Check if the player is new or returning
    lock (Game.UsersInGracePeriod)
    {
        isNewPlayer = !Game.UsersInGracePeriod.Remove(playerName);
    }

    if (isNewPlayer)
    {
        var msg = new { type = "NewPlayerJoined", player = playerName };
        var segment = Game.MessageToSegment(msg);

        foreach (var (player, socket) in Game.ConnectedUsers[guid])
        {
            if (player == playerName) continue;
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    var buffer = new byte[1024 * 4];
    while (webSocket.State == WebSocketState.Open)
    {
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Text)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received message: {message}");
        }
        else if (result.MessageType == WebSocketMessageType.Close)
        {
            break;
        }
    }

    lock (Game.UsersInGracePeriod)
    {
        Game.UsersInGracePeriod[playerName] = DateTime.Now;
    }

    var gracePeriod = Task.Delay(5010);
    await gracePeriod;

    bool playerLeft = false;

    lock (Game.UsersInGracePeriod)
    {
        if (Game.UsersInGracePeriod.TryGetValue(playerName, out DateTime t))
        {
            Console.Write("More than 5 seconds? ");
            Console.WriteLine(DateTime.Now - t >= TimeSpan.FromSeconds(5));
            Console.WriteLine(DateTime.Now - t);
        }

        if (Game.UsersInGracePeriod.TryGetValue(playerName, out DateTime disconnectedTime)
            && DateTime.Now - disconnectedTime >= TimeSpan.FromSeconds(5))
        {
            playerLeft = true;
            Game.UsersInGracePeriod.Remove(playerName);
        }
    }

    if (playerLeft)
    {
        lock (Game.ConnectedUsers)
        {
            Game.ConnectedUsers[guid].Remove(playerName);
            if (Game.ConnectedUsers[guid].Count == 0)
            {
                Game.ConnectedUsers.Remove(guid); // Remove the game entry if no players are connected
            }
        }
    }
}

