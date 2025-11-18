using System;
using System.IO;

public static class Constants
{
    private static string DefaultFilename => "DizimoDatabase.db3";

    // Returns the filename or path provided by the env var, or the default filename.
    public static string DatabaseFilename => Environment.GetEnvironmentVariable("VALIDATE_DB_FILENAME") ?? DefaultFilename;

    // If the env var contains an absolute path, use it directly. Otherwise place the DB
    // inside the project's Data directory (next to this tool). This keeps DB files in the repo
    // folder instead of the system temp directory by default.
    public static string DatabasePath
    {
        get
        {
            var filenameOrPath = DatabaseFilename;

            // If the value is an absolute path, use it as-is.
            if (Path.IsPathRooted(filenameOrPath))
            {
                return $"Data Source={filenameOrPath}";
            }

            // Otherwise, store inside a Data folder located at the application's base directory.
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            var fullPath = Path.Combine(dataDir, filenameOrPath);
            return $"Data Source={fullPath}";
        }
    }
}
