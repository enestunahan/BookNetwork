using BookNetwork.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookNetwork.Persistence.Configurations.Identity;

public sealed class EndpointConfiguration : IEntityTypeConfiguration<Endpoint>
{
    public void Configure(EntityTypeBuilder<Endpoint> builder)
    {
        builder.ToTable("Endpoints");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ActionType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.HttpType).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Definition).HasMaxLength(250).IsRequired();
        builder.Property(e => e.Menu).HasMaxLength(100).IsRequired();

        builder.HasIndex(e => e.Code).IsUnique();

        builder.HasMany(e => e.Roles)
            .WithMany(r => r.Endpoints)
            .UsingEntity("RoleEndpoints");
    }
}
