using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebAPI.Database.Configurations;

internal sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(enrollment => new { enrollment.StudentId, enrollment.CourseId });

        builder.HasOne<Student>().WithMany().HasForeignKey(enrollment => enrollment.StudentId);
        builder.HasOne<Course>().WithMany().HasForeignKey(enrollment => enrollment.CourseId);
    }
}
