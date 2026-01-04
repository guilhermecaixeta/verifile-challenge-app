using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using VerifileChallengeApp.HttpClients.TWFU.Response;

namespace VerifileChallengeApp.HttpClients.TWFU;

public class TWFUService
{
    private readonly HttpClient httpClient;
    private readonly TWFUSettings settings;

    public TWFUService(HttpClient httpClient, IOptions<TWFUSettings> settings)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(settings.Value);

        this.httpClient = httpClient;
        this.settings = settings.Value;
    }

    public async IAsyncEnumerable<Person> GetPersonAsync(string? clientId)
    {
        var queryString = CreateQuery(settings.SecretKey, clientId ?? settings.DefaultPersonId);

        var response = await httpClient.GetAsync($"/api/getPerson{queryString}");

        if (response.IsSuccessStatusCode)
        {
            await foreach (var person in response.Content.ReadFromJsonAsAsyncEnumerable<Person>())
            {
                if (person == null)
                {
                    continue;
                }

                yield return person;
            }
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
