using BookCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Data.Configurations;

public class BookReviewConfiguration : IEntityTypeConfiguration<BookReview>
{
    public void Configure(EntityTypeBuilder<BookReview> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReviewerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ReviewText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.HasOne(r => r.Book)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("BookReviews", t =>
            t.HasCheckConstraint("CK_BookReview_Rating", "\"Rating\" BETWEEN 1 AND 5"));
    }
}
