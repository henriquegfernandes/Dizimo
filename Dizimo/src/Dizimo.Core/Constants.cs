using System;
using System.IO;

namespace Dizimo.Core;

public static class Constants
{
    public static string DatabaseFilename => "dizimo.db3";

    public static string DatabasePath
    {
        get
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(folder)) folder = Directory.GetCurrentDirectory();
            return Path.Combine(folder, DatabaseFilename);
        }
    }
}
