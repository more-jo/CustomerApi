using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public class CustomerApiFactory : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    var configuration = new ConfigurationBuilder()
        .AddUserSecrets<CustomerApiFactory>()
        .AddEnvironmentVariables()
        .Build();

    var connectionString = configuration.GetConnectionString("TestDb") ?? throw new("Connection String 'TestDb' fehlt. Siehe README, Abschnitt Tests.");

    builder.UseSetting("ConnectionStrings:CustomerDb", connectionString);
  }
}