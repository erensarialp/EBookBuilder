using EBookBuilder.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EBookBuilder.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Paper> Papers => Set<Paper>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // BOOKS
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(x => x.PdfPath)
                .HasMaxLength(500);
        });

        // PAPERS
        modelBuilder.Entity<Paper>(entity =>
        {
            entity.ToTable("Papers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.OriginalFilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.OrderIndex)
                .IsRequired();

            // Aynı kitap içinde aynı sıra numarası tekrar edemesin
            entity.HasIndex(x => new
            {
                x.BookId,
                x.OrderIndex
            })
            .IsUnique();

            // Book 1 --- N Paper
            entity.HasOne(x => x.Book)
                .WithMany(x => x.Papers)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}