using Microsoft.EntityFrameworkCore;
using TodoCharacters.Application;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Infrastructure;
using TodoCharacters.Infrastructure.ExternalApi;
using TodoCharacters.Infrastructure.Persistence;
using TodoCharacters.Infrastructure.Persistence.Repositories;
using TodoCharacters.WebApi.Endpoints;
using TodoCharacters.WebApi.Testing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

var useInMemory = builder.Environment.IsEnvironment("Testing");

if (!useInMemory && builder.Environment.IsDevelopment())
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    useInMemory = !await SqlServerHelper.IsAvailableAsync(connectionString);
    if (useInMemory)
    {
        Console.WriteLine(
            "AVISO: SQL Server indisponível em localhost:1433. " +
            "Usando banco InMemory para Development. " +
            "Execute 'docker compose up -d' para persistência real.");
    }
}

if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase(
            builder.Environment.IsEnvironment("Testing") ? "TodoCharactersE2E" : "TodoCharactersDev"));

    builder.Services.AddScoped<ITaskRepository, TaskRepository>();

    if (builder.Environment.IsEnvironment("Testing"))
        builder.Services.AddSingleton<ICharacterApiClient, FakeCharacterApiClient>();
    else
        RegisterRickAndMortyClient(builder);
}
else
{
    builder.Services.AddInfrastructure(builder.Configuration);
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (useInMemory)
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.MapTaskEndpoints();
app.MapCharacterEndpoints();

app.Run();

static void RegisterRickAndMortyClient(WebApplicationBuilder builder)
{
    builder.Services.Configure<RickAndMortyOptions>(
        builder.Configuration.GetSection(RickAndMortyOptions.SectionName));
    builder.Services.AddHttpClient<ICharacterApiClient, RickAndMortyClient>();
}

public partial class Program;

internal static class SqlServerHelper
{
    public static async Task<bool> IsAvailableAsync(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var context = new AppDbContext(options);
            return await context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}
