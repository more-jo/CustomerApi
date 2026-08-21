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

        const string ORDERS_ROUTE = "/orders";

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

        app.MapGet(ORDERS_ROUTE, (int customerId, IOrderRepository repo) =>
        {
            List<Order> orders = repo.GetOrderByCustomerId(customerId);

            return orders;
        });

        app.MapGet(ORDERS_ROUTE + "/{id:int}", (int id, IOrderRepository repo) =>
        {
            var order = repo.GetOrderByOrderId(id);

            return order is not null ? Results.Ok(order) : Results.NotFound();
        });

        app.MapPost(ORDERS_ROUTE, (CreateOrderRequest request, IOrderRepository orderRepo, ICustomerRepository customerRepo) =>
        {
            var customer = customerRepo.GetCustomerById(request.CustomerId);
            if (customer is null)
            {
                return Results.NotFound();
            }

            var maxId = orderRepo.GetMaxId();
            var orderId = maxId + 1;
            var newOrder = new Order(orderId, request.CustomerId, request.Amount);
            orderRepo.Add(newOrder);

            return Results.Created($"{ORDERS_ROUTE}/{newOrder.Id}", newOrder);
        });

        app.MapDelete(ORDERS_ROUTE + "/{id:int}", (int id, IOrderRepository orderRepo) =>
        {
            if (orderRepo.Delete(id))
            {
                return Results.NoContent();
            }

            return Results.NotFound();
        });

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
