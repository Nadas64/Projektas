using backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

var app = builder.Build();

// Automatically applies pending migrations to the database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();

var client = new HttpClient();
var apiKey = builder.Configuration["FINNHUB_API_KEY"];

app.MapGet("/api/quote/{symbol}", async (string symbol) =>
{
    var response = await client.GetAsync($"https://finnhub.io/api/v1/quote?symbol={symbol}&token={apiKey}");
    return Results.Content(await response.Content.ReadAsStringAsync(), "application/json");
});

app.MapGet("/api/symbol/search", async (string query) =>
{
    var response = await client.GetAsync($"https://finnhub.io/api/v1/search?q={query}&exchange=US&token={apiKey}");
    return Results.Content(await response.Content.ReadAsStringAsync(), "application/json");
});

app.Run();
