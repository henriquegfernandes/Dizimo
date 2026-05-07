namespace Dizimo.Services;

public class DataPathProvider : IDataPathProvider
{
    private const string DatabaseFileName = "dizimo.db";
    private readonly string _appDataDirectory;

    public DataPathProvider()
    {
        _appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Dizimo");
        
        // Criar diretório se não existir
        if (!Directory.Exists(_appDataDirectory))
            Directory.CreateDirectory(_appDataDirectory);
    }

    public string GetAppDataDirectory()
    {
        return _appDataDirectory;
    }

    public string GetDatabasePath()
    {
        return Path.Combine(GetAppDataDirectory(), DatabaseFileName);
    }
}
