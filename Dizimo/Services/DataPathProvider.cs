namespace Dizimo.Services;

public class DataPathProvider : IDataPathProvider
{
    private const string DatabaseFileName = "dizimo.db";

    public string GetAppDataDirectory()
    {
        return FileSystem.AppDataDirectory;
    }

    public string GetDatabasePath()
    {
        return Path.Combine(GetAppDataDirectory(), DatabaseFileName);
    }
}
