using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using PeopleCounter.Application;
using PeopleCounter.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PeopleCounter.WebApi.Configuration;

public static class DependencyInjectionConfiguration
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<CounterDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
        builder.Services.AddScoped<IApplicationStore, ApplicationStore>();
        builder.Services.AddControllers();
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        }).AddApiExplorer(options => { options.GroupNameFormat = "'v'VVV"; options.SubstituteApiVersionInUrl = true; });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options => options.CustomSchemaIds(type => type.FullName));
        builder.Services.AddHealthChecks().AddDbContextCheck<CounterDbContext>();
        builder.Services.AddCors(options => options.AddPolicy("default", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
        return builder;
    }

    public static WebApplication ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
        app.UseCors("default");
        app.MapHealthChecks("/api/health");
        app.MapControllers();
        using var scope = app.Services.CreateScope();
        SeedData.EnsureSeededAsync(scope.ServiceProvider.GetRequiredService<CounterDbContext>()).GetAwaiter().GetResult();
        return app;
    }
}
