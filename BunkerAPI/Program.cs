using BunkerAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Render (и другие PaaS) задают порт через переменную PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddSingleton<ICardService, CardService>();
builder.Services.AddSingleton<IGameSessionService, GameSessionService>();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
