using BookNetwork.Domain.Entities;
using BookNetwork.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookNetwork.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasMany(c => c.BookCategories)
            .WithOne(bc => bc.Category)
            .HasForeignKey(bc => bc.CategoryId);

        builder.HasData(SeedData.Categories);
    }
}
