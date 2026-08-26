using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Characters.Dtos;
using TodoCharacters.Application.Characters.Queries;
using TodoCharacters.Application.Tasks.Dtos;
using TodoCharacters.Infrastructure.Persistence;

namespace TodoCharacters.FunctionalTests;

internal sealed class StubCharacterApiClient : ICharacterApiClient
{
    public Task<CharacterPageDto> GetCharactersAsync(int page, CancellationToken cancellationToken = default) =>
        Task.FromResult(new CharacterPageDto([], page, 1, 0, false, false));

    public Task<CharacterDetailDto?> GetCharacterByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult<CharacterDetailDto?>(null);
}

internal sealed class FailingCharacterApiClient : ICharacterApiClient
{
    public Task<CharacterPageDto> GetCharactersAsync(int page, CancellationToken cancellationToken = default) =>
        throw new ExternalApiException("EXTERNAL_API_UNAVAILABLE", "Indisponível");

    public Task<CharacterDetailDto?> GetCharacterByIdAsync(int id, CancellationToken cancellationToken = default) =>
        throw new ExternalApiException("EXTERNAL_API_UNAVAILABLE", "Indisponível");
}

public class TaskApiFunctionalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> CreateFactory(string dbName) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                        || d.ServiceType == typeof(AppDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                var apiDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICharacterApiClient));
                if (apiDescriptor is not null) services.Remove(apiDescriptor);
                services.AddSingleton<ICharacterApiClient, StubCharacterApiClient>();
            });
        });

    [Fact]
    public async Task POST_task_with_invalid_title_returns_400()
    {
        var client = CreateFactory(nameof(POST_task_with_invalid_title_returns_400)).CreateClient();
        var response = await client.PostAsJsonAsync("/api/tasks", new { titulo = "ab" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Complete_parent_with_pending_child_returns_409()
    {
        var factory = CreateFactory(nameof(Complete_parent_with_pending_child_returns_409));
        var client = factory.CreateClient();

        var parentResponse = await client.PostAsJsonAsync("/api/tasks", new { titulo = "Pai func", descricao = (string?)null, tarefaPaiId = (Guid?)null });
        parentResponse.EnsureSuccessStatusCode();
        var parent = await parentResponse.Content.ReadFromJsonAsync<TaskDto>();

        await client.PostAsJsonAsync("/api/tasks", new { titulo = "Filho func", descricao = (string?)null, tarefaPaiId = parent!.Id });

        var patch = await client.PatchAsJsonAsync($"/api/tasks/{parent.Id}/status", new { status = "Concluído" });
        patch.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GET_characters_when_external_api_fails_returns_503()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICharacterApiClient));
                if (descriptor is not null) services.Remove(descriptor);
                services.AddSingleton<ICharacterApiClient, FailingCharacterApiClient>();
            });
        });

        var response = await factory.CreateClient().GetAsync("/api/characters?page=1");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
