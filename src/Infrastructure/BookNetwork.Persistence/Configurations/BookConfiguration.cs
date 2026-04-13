using BookNetwork.Domain.Entities;
using BookNetwork.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookNetwork.Persistence.Configurations;

public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Isbn)
            .HasMaxLength(13)
            .IsRequired();

        builder.HasIndex(b => b.Isbn)
            .IsUnique();

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.BookAuthors)
            .WithOne(ba => ba.Book)
            .HasForeignKey(ba => ba.BookId);

        builder.HasMany(b => b.BookCategories)
            .WithOne(bc => bc.Book)
            .HasForeignKey(bc => bc.BookId);

        builder.HasData(SeedData.Books);
    }
}
