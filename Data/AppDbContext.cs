using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<BorrowRecord> BorrowRecords { get; set; }
    public DbSet<Fine> Fines { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Book>()
            .HasIndex(b => b.ISBN).IsUnique();

        builder.Entity<Fine>()
            .Property(f => f.Amount).HasColumnType("decimal(18,2)");

        builder.Entity<BorrowRecord>()
            .HasOne(b => b.Fine)
            .WithOne(f => f.BorrowRecord)
            .HasForeignKey<Fine>(f => f.BorrowRecordId);

        // Seed Data
        builder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884", Category = "Software Engineering", Publisher = "Prentice Hall", PublishedYear = 2008, TotalCopies = 5, AvailableCopies = 5 },
            new Book { Id = 2, Title = "Design Patterns", Author = "GoF", ISBN = "9780201633610", Category = "Software Engineering", Publisher = "Addison-Wesley", PublishedYear = 1994, TotalCopies = 3, AvailableCopies = 3 },
            new Book { Id = 3, Title = "Introduction to Algorithms", Author = "CLRS", ISBN = "9780262033848", Category = "Core CS", Publisher = "MIT Press", PublishedYear = 2009, TotalCopies = 4, AvailableCopies = 4 },
            new Book { Id = 4, Title = "Database System Concepts", Author = "Silberschatz", ISBN = "9780073523323", Category = "Databases", Publisher = "McGraw-Hill", PublishedYear = 2019, TotalCopies = 6, AvailableCopies = 6 }
        );
    }
}
