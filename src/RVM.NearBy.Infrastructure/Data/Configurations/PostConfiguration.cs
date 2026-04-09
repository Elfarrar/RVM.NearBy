using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.NearBy.Domain.Entities;

namespace RVM.NearBy.Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Content).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.LocationName).HasMaxLength(200);
        builder.Property(p => p.Visibility).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(p => p.Place)
            .WithMany(pl => pl.Posts)
            .HasForeignKey(p => p.PlaceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Media)
            .WithOne(m => m.Post)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Likes)
            .WithOne(l => l.Post)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.CreatedAt);
    }
}
