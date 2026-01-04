using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using VerifileChallengeApp.HttpClients.TWFU.Response;

namespace VerifileChallengeApp.HttpClients.TWFU;

public class TWFUService
{
    private readonly HttpClient httpClient;
    private readonly TWFUSettings settings;
    private readonly ILogger<TWFUService> logger;

    public TWFUService(HttpClient httpClient, IOptions<TWFUSettings> settings, ILogger<TWFUService> logger)
    {
        this.settings = settings.Value;
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async IAsyncEnumerable<Person> GetPersonAsync(string? clientId)
    {
        logger.LogInformation("Starting request");

        var queryString = CreateQuery(settings.SecretKey, clientId ?? settings.DefaultPersonId);

        var response = await httpClient.GetAsync($"/api/getPerson{queryString}");

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("The request was not succeeded status code'{0}'", response.StatusCode);
            yield break;
        }

        await foreach (var person in response.Content.ReadFromJsonAsAsyncEnumerable<Person>())
        {
            if (person == null)
            {
                continue;
            }

            yield return person;
        }
    }

    private static QueryString CreateQuery(string secretKey, string clientId) =>
        new QueryBuilder
        {
            { "id", clientId },
            { "key", secretKey },
            { "output", "json" }
        }.ToQueryString();
}
