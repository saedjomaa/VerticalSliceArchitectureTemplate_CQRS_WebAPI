using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Database;

/// <summary>
/// Database-provider error codes in one place, so handlers never repeat SQLite numbers.
/// </summary>
internal static class SqliteErrors
{
    private const int ConstraintPrimaryKey = 1555;

    /// <summary>True when saving failed only because <paramref name="entity"/> violated a primary key.</summary>
    public static bool IsPrimaryKeyViolation(this DbUpdateException exception, object entity) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: ConstraintPrimaryKey } &&
        exception.Entries.Count == 1 && ReferenceEquals(exception.Entries[0].Entity, entity);
}
