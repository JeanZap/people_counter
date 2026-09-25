using PeopleCounter.WebApi.Configuration;

WebApplication.CreateBuilder(args)
    .AddServices()
    .Build()
    .ConfigureApp()
    .Run();

public partial class Program { }
