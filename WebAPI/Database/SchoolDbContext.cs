using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Database;

/// <summary>
/// The only data-access type. Handlers use it directly (no repositories, no IApplicationDbContext):
/// each slice writes the query it needs.
/// </summary>
internal sealed class SchoolDbContext(DbContextOptions<SchoolDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolDbContext).Assembly);
        SchoolSeed.Configure(modelBuilder);
    }
}
