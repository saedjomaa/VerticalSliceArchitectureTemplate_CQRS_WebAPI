using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPI.Database.Migrations;

[DbContext(typeof(SchoolDbContext))]
[Migration("20261120061000_InitialSchool")]
public sealed class InitialSchool : Migration
{
    private static readonly Guid SaraId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid OmarId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly Guid CSharpId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1");
    private static readonly Guid DatabaseId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2");
    private static readonly Guid WebApiId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa3");
    private static readonly DateTimeOffset InitialEnrollmentDate = new(2026, 11, 1, 9, 0, 0, TimeSpan.Zero);

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Students", row => row.Id));

        migrationBuilder.CreateTable(
            name: "Courses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Courses", row => row.Id));

        migrationBuilder.CreateTable(
            name: "Enrollments",
            columns: table => new
            {
                StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                EnrolledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Enrollments", row => new { row.StudentId, row.CourseId });
                table.ForeignKey("FK_Enrollments_Students_StudentId", row => row.StudentId,
                    "Students", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Enrollments_Courses_CourseId", row => row.CourseId,
                    "Courses", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData("Students", ["Id", "FullName"], new object[,]
        {
            { SaraId, "Sara Ahmed" },
            { OmarId, "Omar Ali" }
        });

        migrationBuilder.InsertData("Courses", ["Id", "Title"], new object[,]
        {
            { CSharpId, "C# Fundamentals" },
            { DatabaseId, "Database Fundamentals" },
            { WebApiId, "ASP.NET Core Web API" }
        });

        migrationBuilder.InsertData("Enrollments", ["StudentId", "CourseId", "EnrolledAt"],
            [SaraId, CSharpId, InitialEnrollmentDate]);

        migrationBuilder.CreateIndex("IX_Enrollments_CourseId", "Enrollments", "CourseId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Enrollments");
        migrationBuilder.DropTable("Courses");
        migrationBuilder.DropTable("Students");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder) => BuildInitialModel(modelBuilder);

    internal static void BuildInitialModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "11.0.0");

        var students = modelBuilder.Entity("Domain.Entities.Student");
        students.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("TEXT");
        students.Property<string>("FullName").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
        students.HasKey("Id");
        students.ToTable("Students");
        students.HasData(new { Id = SaraId, FullName = "Sara Ahmed" }, new { Id = OmarId, FullName = "Omar Ali" });

        var courses = modelBuilder.Entity("Domain.Entities.Course");
        courses.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("TEXT");
        courses.Property<string>("Title").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
        courses.HasKey("Id");
        courses.ToTable("Courses");
        courses.HasData(
            new { Id = CSharpId, Title = "C# Fundamentals" },
            new { Id = DatabaseId, Title = "Database Fundamentals" },
            new { Id = WebApiId, Title = "ASP.NET Core Web API" });

        var enrollments = modelBuilder.Entity("Domain.Entities.Enrollment");
        enrollments.Property<Guid>("StudentId").HasColumnType("TEXT");
        enrollments.Property<Guid>("CourseId").HasColumnType("TEXT");
        enrollments.Property<DateTimeOffset>("EnrolledAt").HasColumnType("TEXT");
        enrollments.HasKey("StudentId", "CourseId");
        enrollments.HasIndex("CourseId");
        enrollments.ToTable("Enrollments");
        enrollments.HasOne("Domain.Entities.Student", null).WithMany()
            .HasForeignKey("StudentId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        enrollments.HasOne("Domain.Entities.Course", null).WithMany()
            .HasForeignKey("CourseId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        enrollments.HasData(new { StudentId = SaraId, CourseId = CSharpId, EnrolledAt = InitialEnrollmentDate });
    }
}
