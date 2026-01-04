using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using Verifile.PubSub;
using VerifileChallengeApp.Database;
using VerifileChallengeApp.Database.Models;
using VerifileChallengeApp.HttpClients.TWFU;

namespace VerifileChallengeApp;

public class GetPersonFunction
{
    private readonly TWFUService service;
    private readonly VerifileDbContext dbContext;
    private readonly ILogger<GetPersonFunction> logger;
    private readonly IPublisher<Database.Models.Person> publisher;

    public GetPersonFunction(
        TWFUService service,
        VerifileDbContext dbContext,
        IPublisher<Person> publisher,
        ILogger<GetPersonFunction> logger)
    {
        this.service = service;
        this.dbContext = dbContext;
        this.publisher = publisher;
        this.logger = logger;
    }

    [Function("GetPerson")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            logger.LogInformation("HTTP trigger function started to process a request.");

            var people = service.GetPersonAsync(req.Query["id"]);

            if (people == null || !await people.AnyAsync())
            {
                return new NotFoundResult();
            }

            var person = await GetLatestUpdated(people);

            await SaveQuarterThreeResult(people);

            logger.LogInformation("HTTP trigger function completed processing the request.");

            return new OkObjectResult(person);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurs while processing the request");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task SaveQuarterThreeResult(IAsyncEnumerable<HttpClients.TWFU.Response.Person> people)
    {
        var peopleOnQuarterThree = await people
            .Where(p => p.LastUpdate.Month is >= 7 and <= 9)
            .ToListAsync();

        logger.LogInformation("Found {Count} people updated in quarter three.", peopleOnQuarterThree.Count);

        if (peopleOnQuarterThree.Count == 0)
        {
            return;
        }

        var peopleOnQuarterThreeKeyed = peopleOnQuarterThree
            .Select(p => $"{p.PersonId}:{p.LastUpdate:yyyy-MM-dd hh:mm:ss.fffffff}")
            .ToList();

        var existingPeople = await dbContext.People
            .Where(p => peopleOnQuarterThreeKeyed.Contains(string.Concat(p.Id.ToString(), ":", p.LastUpdate.ToString())))
            .Select(p => new { p.Id, p.LastUpdate })
            .ToHashSetAsync();

        var newPeople = peopleOnQuarterThree
                            .ExceptBy(existingPeople.Select(p => (p.Id, p.LastUpdate)), p => (p.PersonId, p.LastUpdate))
                            .Select(Person.FromDto);

        if (!newPeople.Any())
        {
            logger.LogInformation("No new people to add to the database.");
            return;
        }

        logger.LogInformation("Adding {Count} new people to the database.", newPeople.Count());

        var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            await dbContext.People.AddRangeAsync(newPeople);

            logger.LogInformation("Saving changes to the database.");

            await dbContext.SaveChangesAsync();

            var tasks = newPeople.Select(publisher.PublishAsync);

            await Task.WhenAll(tasks);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing people updated in quarter three.");
            await transaction.RollbackAsync();
        }
    }

    private static async Task<HttpClients.TWFU.Response.Person> GetLatestUpdated(IAsyncEnumerable<HttpClients.TWFU.Response.Person> response) =>
        await response.OrderBy(p => p.LastUpdate).LastAsync();
}
