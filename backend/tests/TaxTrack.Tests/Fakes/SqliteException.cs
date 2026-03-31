namespace Microsoft.Data.Sqlite;

public sealed class SqliteException(int sqliteErrorCode, int sqliteExtendedErrorCode) : Exception("SQLITE_CONSTRAINT")
{
    public int SqliteErrorCode { get; } = sqliteErrorCode;

    public int SqliteExtendedErrorCode { get; } = sqliteExtendedErrorCode;
}
