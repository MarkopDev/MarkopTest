using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.HasKey(news => news.Id);
        builder.Property(news => news.Title).IsRequired();
        builder.Property(news => news.Preview).IsRequired();
        builder.Property(news => news.Content).IsRequired();
        builder.Property(news => news.Id).ValueGeneratedOnAdd();
        builder.Property(u => u.IsHidden).HasDefaultValue(true);
    }
}