using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Database;

internal static class SchoolSeed
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        var saraId = Guid.Parse("11111111-1111-4111-8111-111111111111");
        var csharpId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1");
        var initialCourseVersion = Guid.Parse("33333333-3333-4333-8333-333333333333");

        modelBuilder.Entity<Student>().HasData(
            new { Id = saraId, FullName = "Sara Ahmed" },
            new { Id = Guid.Parse("22222222-2222-4222-8222-222222222222"), FullName = "Omar Ali" });

        modelBuilder.Entity<Course>().HasData(
            new { Id = csharpId, Title = "C# Fundamentals", Capacity = Course.DefaultCapacity,
                EnrolledCount = 1, RowVersion = initialCourseVersion },
            new { Id = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2"), Title = "Database Fundamentals",
                Capacity = Course.DefaultCapacity, EnrolledCount = 0, RowVersion = initialCourseVersion },
            new { Id = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa3"), Title = "ASP.NET Core Web API",
                Capacity = Course.DefaultCapacity, EnrolledCount = 0, RowVersion = initialCourseVersion });

        modelBuilder.Entity<Enrollment>().HasData(new
        {
            StudentId = saraId,
            CourseId = csharpId,
            EnrolledAt = new DateTimeOffset(2026, 11, 1, 9, 0, 0, TimeSpan.Zero)
        });
    }
}
