var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithHostPort(1433);

var db = sql.AddDatabase("Default");


builder.AddAzureFunctionsProject<Projects.VerifileChallengeApp>("verifilechallengeapp")
     .WithReference(db)
     .WaitFor(db);

builder.Build().Run();
