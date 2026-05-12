namespace Dizimo.Data;

public static class Constants
{
    public const string DatabaseFilename = "AppSQLite.db3";

    public static string DatabasePath
    {
        get
        {
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Dizimo");
            return $"Data Source={Path.Combine(appDataDir, DatabaseFilename)}";
        }
    }
}