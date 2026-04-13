using BookNetwork.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookNetwork.Persistence.Contexts;

public class BookNetworkDbContext(DbContextOptions<BookNetworkDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //burası reflection yapılmış hali
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookNetworkDbContext).Assembly);
    }
}
