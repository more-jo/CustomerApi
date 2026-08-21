using CustomerApi;
using Microsoft.EntityFrameworkCore;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // scoped since it depends on CustomerDbContext which is scoped
        // earlier version used in memory singleton. That was replaced with per instance memory
        // that gives in the tests an isolated data store.
        ConfigureDatabase(builder.Services);
        builder.Services.AddScoped<ICustomerRepository, EfCoreCustomerRepository>();
        builder.Services.AddScoped<IOrderRepository, EfCoreOrderRepository>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapGet("/throw", () =>
                {
                    throw new InvalidOperationException();
                }
            );
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();

        app.MapCustomerEndpoints();

        app.MapOrderEndpoints();

        app.Run();
    }

    private static void ConfigureDatabase(IServiceCollection services)
    {
        var dbCustomer = $"CustomerDb-{Guid.NewGuid()}";

        // this only shows how to create the database - it does not create the database itself.
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseInMemoryDatabase(dbCustomer)
        );
    }
}

public partial class Program { }
