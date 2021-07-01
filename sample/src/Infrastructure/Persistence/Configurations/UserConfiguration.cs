using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedOnAdd();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.UserName).HasDefaultValue("");
            builder.Property(u => u.IsEnable).HasDefaultValue(true);
            builder.Property(u => u.EmailConfirmed).HasDefaultValue(false);
            builder.Property(u => u.NormalizedUserName).HasDefaultValue("");
            builder.Property(u => u.PhoneNumberConfirmed).HasDefaultValue(false);
            builder.HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
            builder.HasIndex(u => u.NormalizedEmail).IsUnique().HasFilter("\"Email\" IS NOT NULL");
            builder.HasIndex(u => u.NormalizedUserName).IsUnique().HasFilter("NOT \"UserName\"=''");
            builder.HasIndex(u => u.PhoneNumber).IsUnique().HasFilter("\"PhoneNumber\" IS NOT NULL");
        }
    }
}