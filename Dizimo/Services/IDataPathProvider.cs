namespace Dizimo.Services;

public interface IDataPathProvider
{
    string GetAppDataDirectory();
    string GetDatabasePath();
}
