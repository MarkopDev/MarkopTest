using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class InitializeHistoryConfiguration : IEntityTypeConfiguration<InitializeHistory>
{
    public void Configure(EntityTypeBuilder<InitializeHistory> builder)
    {
        builder.HasKey(s => s.Version);

        builder.Property(n => n.DateTime)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();
    }
}