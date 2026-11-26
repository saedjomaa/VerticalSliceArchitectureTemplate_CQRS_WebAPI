using System.Linq.Expressions;
using Domain.Entities;

namespace WebAPI.Features.Students;

internal static class StudentMappings
{
    public static readonly Expression<Func<Student, StudentResponse>> ToResponse = student =>
        new StudentResponse(student.Id, student.FullName);
}
