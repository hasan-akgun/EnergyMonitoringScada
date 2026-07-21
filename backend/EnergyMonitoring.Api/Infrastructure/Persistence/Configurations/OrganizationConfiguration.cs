using EnergyMonitoring.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyMonitoring.Api.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasOne(x => x.ParentOrganization)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ParentOrganizationId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}