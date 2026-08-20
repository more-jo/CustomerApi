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

        const string CUSTOMER_ROUTE = "/customers";
        const string ORDERS_ROUTE = "/orders";

        var app = builder.Build();

        SeedDataBase(app);

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

        app.MapGet(CUSTOMER_ROUTE, (ICustomerRepository repo) =>
        {
            List<Customer> customers = repo.GetAll();
            // Constructor injection: ASP.NET matches the parameter type (ICustomerRepository)
            // to the registered service and passes the instance here.
            return customers;
        });

        app.MapGet(CUSTOMER_ROUTE + "/{id:int}", (int id, ICustomerRepository repo) =>
        {
            var customer = repo.GetCustomerById(id);
            return customer is not null ? Results.Ok(customer) : Results.NotFound();
        });

        app.MapPost(CUSTOMER_ROUTE, (CreateCustomerRequest newCustomer, ICustomerRepository repo) =>
        {
            var validationResult = CustomerRequestValidator.ValidateCustomerName(newCustomer);
            if (validationResult is not null)
            {
                return validationResult;
            }

            int newId = repo.GetMaxId() + 1;
            var customer = new Customer(newId, newCustomer.Name);
            repo.Add(customer);

            return Results.Created($"{CUSTOMER_ROUTE}/{customer.Id}", customer);
        });

        app.MapPut(CUSTOMER_ROUTE + "/{id:int}", (int id, UpdateCustomerRequest newCustomer, ICustomerRepository repo) =>
        {
            var validationResult = CustomerRequestValidator.ValidateCustomerName(newCustomer);
            if (validationResult is not null)
            {
                return validationResult;
            }

            if (repo.Update(id, newCustomer.Name))
            {
                return Results.NoContent();
            }

            return Results.NotFound();
        });

        app.MapDelete(CUSTOMER_ROUTE + "/{id:int}", (int id, ICustomerRepository repo) =>
        {
            if (repo.Delete(id))
            {
                return Results.NoContent();
            }

            return Results.NotFound();
        });

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

    private static void SeedDataBase(Microsoft.AspNetCore.Builder.WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
            db.Database.EnsureCreated();

            if (!db.Customers.Any())
            {
                db.Customers.AddRange(
                    new Customer(1, "Alice"),
                    new Customer(2, "Bob")
                );
            }

            db.SaveChanges();
        }
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
