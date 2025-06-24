using System.Text.Json.Serialization;
using DAL.Context;
using DAL.Repositories;
using UnoGame.Core.Config;
using UnoGame.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Interfaces;
using WebApp.Handlers;
using WebApp.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UnoDbContext>(options =>
{
    options.EnableSensitiveDataLogging();
    options.UseNpgsql(connectionString);
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<GameService, GameService>();
builder.Services.AddScoped<UserService, UserService>();

builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();

builder.Services.Configure<UserLimitsConfig>(builder.Configuration.GetSection("UserLimits"));
builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("Email"));

builder.Services.AddSession(options =>
{
    // options.Cookie.Name = "YourSessionCookieName";
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Set the session timeout duration
});

var origins = builder.Configuration
    .GetSection("AllowedOrigins")
    .GetChildren()
    .Select(child => child.Value!)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCors", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();

builder.Services.AddAuthentication("UnoToken")
    .AddScheme<AuthenticationSchemeOptions, UnoTokenAuthenticationHandler>("UnoToken", null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("AllowCors");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.MapHub<GameHub>("/gamehub");

app.Run();
