using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Characters.Dtos;
using TodoCharacters.Infrastructure.Persistence;

namespace TodoCharacters.IntegrationTests.ExternalApi;

internal sealed class FakeCharacterApiClient : ICharacterApiClient
{
    public Task<CharacterPageDto> GetCharactersAsync(int page, CancellationToken cancellationToken = default) =>
        Task.FromResult(new CharacterPageDto(
            [new CharacterSummaryDto(1, "Rick", "Alive", "Human", "http://img")],
            page,
            2,
            2,
            page < 2,
            page > 1));

    public Task<CharacterDetailDto?> GetCharacterByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult<CharacterDetailDto?>(new CharacterDetailDto(
            id, "Rick", "Alive", "Human", "http://img", "Male", "Earth", "Citadel", 3));
}

public class CharacterApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CharacterApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor is not null) services.Remove(dbDescriptor);
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationCharacters"));

                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICharacterApiClient));
                if (descriptor is not null) services.Remove(descriptor);
                services.AddSingleton<ICharacterApiClient, FakeCharacterApiClient>();
            });
        });
    }

    [Fact]
    public async Task Get_characters_should_return_internal_pagination_contract()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/characters?page=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("rickandmortyapi.com");
        json.Should().Contain("hasNext");
        json.Should().Contain("items");
    }
}
