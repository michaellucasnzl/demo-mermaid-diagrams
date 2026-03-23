using BookCatalog.Application.Interfaces;
using BookCatalog.Application.Services;
using BookCatalog.Infrastructure.Data;
using BookCatalog.Infrastructure.ExternalApis;
using BookCatalog.Infrastructure.Repositories;
using BookCatalog.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpClient<IOpenLibraryService, OpenLibraryService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "BookCatalog-Demo/1.0 (github.com/demo-mermaid-diagrams)");
        });

        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IPublisherRepository, PublisherRepository>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<ICatalogService, CatalogService>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }
}
