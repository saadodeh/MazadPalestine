using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MzadPalestine.Infrastructure.Data;

namespace MzadPalestine.Tests;

public class TestDatabaseFixture
{
    private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=MzadPalestineTests;Trusted_Connection=True;MultipleActiveResultSets=true";
    public readonly IServiceProvider ServiceProvider;

    public TestDatabaseFixture()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(ConnectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        ServiceProvider = services.BuildServiceProvider();

        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
    }
}
