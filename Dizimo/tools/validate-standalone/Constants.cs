using System;
using System.IO;

public static class Constants
{
    public static string DatabaseFilename => Environment.GetEnvironmentVariable("VALIDATE_DB_FILENAME") ?? "ValidateSQLite.db3";
    public static string DatabasePath => $"Data Source={Path.Combine(Path.GetTempPath(), DatabaseFilename)}";
}
