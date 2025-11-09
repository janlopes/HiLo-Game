using HiLoGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Infrastructure (EF Core + state store via IDistributedCache)
var conn = builder.Configuration.GetConnectionString("Default");
builder.Services.AddHiLoInfrastructure(conn);
builder.Services.AddGameDependecyInjection();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o =>
{
    o.AddPolicy("WithCreds", p =>
        p
        .SetIsOriginAllowed(_ => true)  // reflect any origin
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

app.UseCors("WithCreds");

// Apply any pending EF Core migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    db.Database.EnsureCreated(); // creates tables based on the current model
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<HiLoGame.Api.Hubs.GameHub>("/hubs/game");

app.Run();

// Needed for WebApplicationFactory<T> in integration tests
public partial class Program { }
