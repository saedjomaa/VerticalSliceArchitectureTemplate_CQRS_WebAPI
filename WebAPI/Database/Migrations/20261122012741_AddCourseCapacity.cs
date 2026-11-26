using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Database.Migrations;

/// <inheritdoc />
public partial class _20261122012741_AddCourseCapacity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Capacity",
            table: "Courses",
            type: "INTEGER",
            nullable: false,
            defaultValue: 30);

        migrationBuilder.AddColumn<int>(
            name: "EnrolledCount",
            table: "Courses",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<Guid>(
            name: "RowVersion",
            table: "Courses",
            type: "TEXT",
            nullable: false,
            defaultValue: new Guid("33333333-3333-4333-8333-333333333333"));

        migrationBuilder.Sql("""
            UPDATE "Courses"
            SET "EnrolledCount" = (
                SELECT COUNT(*) FROM "Enrollments" WHERE "Enrollments"."CourseId" = "Courses"."Id"
            );

            UPDATE "Courses"
            SET "Capacity" = MAX("Capacity", "EnrolledCount");
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Capacity",
            table: "Courses");

        migrationBuilder.DropColumn(
            name: "EnrolledCount",
            table: "Courses");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "Courses");
    }
}
