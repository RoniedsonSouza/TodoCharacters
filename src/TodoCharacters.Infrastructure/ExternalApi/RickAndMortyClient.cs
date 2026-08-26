using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Characters.Dtos;
using TodoCharacters.Application.Characters.Queries;
using TodoCharacters.Infrastructure.ExternalApi;

namespace TodoCharacters.Infrastructure.ExternalApi;

public sealed class RickAndMortyClient : ICharacterApiClient
{
    private readonly HttpClient _httpClient;

    public RickAndMortyClient(HttpClient httpClient, IOptions<RickAndMortyOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
    }

    public async Task<CharacterPageDto> GetCharactersAsync(int page, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync($"character?page={page}", cancellationToken);
        }
        catch (TaskCanceledException)
        {
            throw new ExternalApiException(
                "EXTERNAL_API_UNAVAILABLE",
                "A integração com personagens está indisponível no momento.");
        }
        catch (HttpRequestException)
        {
            throw new ExternalApiException(
                "EXTERNAL_API_UNAVAILABLE",
                "A integração com personagens está indisponível no momento.");
        }

        using (response)
        {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new CharacterPageDto(
                Array.Empty<CharacterSummaryDto>(),
                page,
                page > 1 ? page - 1 : 1,
                0,
                false,
                page > 1);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalApiException(
                "EXTERNAL_API_UNAVAILABLE",
                "A integração com personagens está indisponível no momento.");
        }

        var payload = await response.Content.ReadFromJsonAsync<RickCharacterPageResponse>(cancellationToken);
        if (payload is null)
        {
            throw new ExternalApiException(
                "EXTERNAL_API_UNAVAILABLE",
                "Resposta inválida da API de personagens.");
        }

        return CharacterMapper.ToPageDto(payload, page);
        }
    }

    public async Task<CharacterDetailDto?> GetCharacterByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync($"character/{id}", cancellationToken);
        }
        catch (TaskCanceledException)
        {
            throw new ExternalApiException(
                "EXTERNAL_API_UNAVAILABLE",
                "A integração com personagens está indisponível no momento.");
        }
        catch (HttpRequestException)
        {
            throw new ExternalApiException(
                "EXTERNAL_API_UNAVAILABLE",
                "A integração com personagens está indisponível no momento.");
        }

        using (response)
        {
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalApiException(
                "EXTERNAL_API_UNAVAILABLE",
                "A integração com personagens está indisponível no momento.");
        }

        var payload = await response.Content.ReadFromJsonAsync<RickCharacterDetailResponse>(cancellationToken);
        return payload is null ? null : CharacterMapper.ToDetailDto(payload);
        }
    }
}

internal sealed class RickCharacterPageResponse
{
    [JsonPropertyName("info")]
    public RickPageInfo Info { get; set; } = new();

    [JsonPropertyName("results")]
    public List<RickCharacterDetailResponse> Results { get; set; } = new();
}

internal sealed class RickPageInfo
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("prev")]
    public string? Prev { get; set; }
}

internal sealed class RickCharacterDetailResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("species")]
    public string Species { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("origin")]
    public RickNamedResource Origin { get; set; } = new();

    [JsonPropertyName("location")]
    public RickNamedResource Location { get; set; } = new();

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("episode")]
    public List<string> Episode { get; set; } = new();
}

internal sealed class RickNamedResource
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
