using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verifile.PubSub;
using VerifileChallengeApp.Database;
using VerifileChallengeApp.Database.Models;
using VerifileChallengeApp.HttpClients.TWFU;

namespace VerifileChallengeApp;

public class GetPersonFunction
{
    private readonly ILogger<GetPersonFunction> logger;
    private readonly IPublisher<Person> publisher;
    private readonly VerifileDbContext dbContext;
    private readonly TWFUService service;

    public GetPersonFunction(
        ILogger<GetPersonFunction> logger,
        IPublisher<Person> publisher,
        VerifileDbContext dbContext,
        TWFUService service)
    {
        this.logger = logger;
        this.publisher = publisher;
        this.dbContext = dbContext;
        this.service = service;
    }

    [Function("GetPerson")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        logger.LogInformation("HTTP trigger function started to process a request.");

        var response = service.GetPersonAsync(req.Query["id"]);

        if (response == null)
        {
            return new NotFoundResult();
        }

        var person = await response.OrderBy(p => p.LastUpdate).LastAsync();

        var peopleOnQuarterThree = await response.Where(p => p.LastUpdate.Month is >= 7 and <= 9).ToListAsync();

        logger.LogInformation("Found {Count} people updated in quarter three.", peopleOnQuarterThree.Count);

        List<Person> peopleAdded = [];

        if (peopleOnQuarterThree.Count == 0)
        {
            return new OkObjectResult(person);
        }

        var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (var personOnQuarterThree in peopleOnQuarterThree)
            {
                logger.LogInformation("Checking person with ID {PersonId} and LastUpdate {LastUpdate}.", personOnQuarterThree.PersonId, personOnQuarterThree.LastUpdate);

                if (!await dbContext.People.AnyAsync(p => p.Id == personOnQuarterThree.PersonId && p.LastUpdate == personOnQuarterThree.LastUpdate))
                {
                    logger.LogInformation("Adding new person with ID {PersonId} and LastUpdate {LastUpdate} to the database.", personOnQuarterThree.PersonId, personOnQuarterThree.LastUpdate);

                    var personEntity = Person.FromDto(personOnQuarterThree);
                    dbContext.People.Add(personEntity);
                    peopleAdded.Add(personEntity);
                }
            }

            logger.LogInformation("Saving changes to the database.");

            await dbContext.SaveChangesAsync();

            var tasks = peopleAdded.Select(publisher.PublishAsync);

            await Task.WhenAll(tasks);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing people updated in quarter three.");
            await transaction.RollbackAsync();
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        logger.LogInformation("HTTP trigger function completed processing the request.");

        return new OkObjectResult(person);
    }
}
