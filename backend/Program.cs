var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var app = builder.Build();
app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

var client = new HttpClient();
var apiKey = builder.Configuration["FINNHUB_API_KEY"];

app.MapGet("/api/quote/{symbol}", async (string symbol) =>
{
    var response = await client.GetAsync($"https://finnhub.io/api/v1/quote?symbol={symbol}&token={apiKey}");
    return Results.Content(await response.Content.ReadAsStringAsync(), "application/json");
});

app.MapGet("/api/symbol/search", async (string query) =>
{
    var response = await client.GetAsync($"https://finnhub.io/api/v1/search?q={query}&token={apiKey}");
    return Results.Content(await response.Content.ReadAsStringAsync(), "application/json");
});

app.Run();
