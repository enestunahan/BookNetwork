using BookNetwork.Application.Common.Repositories;
using BookNetwork.Application.Common.Repositories.Books;
using BookNetwork.Persistence.Contexts;
using BookNetwork.Persistence.Repositories.Books;
using BookNetwork.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookNetwork.Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookNetworkDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

            options.UseNpgsql(connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(BookNetworkDbContext).Assembly.FullName));
        });

        services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
        services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));
        services.AddScoped<IBookReadRepository, BookReadRepository>();
        services.AddScoped<IBookWriteRepository, BookWriteRepository>();

        return services;
    }
}
