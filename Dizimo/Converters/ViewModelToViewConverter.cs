using Avalonia.Data.Converters;

namespace Dizimo.Converters;

/// <summary>
/// Converter que mapeia ViewModels para Views usando Convention-based Naming.
/// Convenção: LoginViewModel → LoginPage, DizimistaListViewModel → DizimistaListPage, etc.
/// Reduz drasticamente a manutenção manual quando novos ViewModels são criados.
/// 
/// Para ViewModels que NÃO seguem a convenção, adicione um mapeamento em ExceptionMappings.
/// </summary>
public class ViewModelToViewConverter : IValueConverter
{
    private static readonly Dictionary<Type, Type?> _typeCache = new();
    
    /// <summary>
    /// Mapeamento de exceções para ViewModels que não seguem a convenção
    /// Exemplo: ShellViewModel → AppShell (ao invés de ShellPage)
    /// </summary>
    private static readonly Dictionary<string, string> ExceptionMappings = new()
    {
        { "ShellViewModel", "AppShell" },              // ShellViewModel → AppShell (não ShellPage)
        { "MainViewModel", "MainPage" },               // MainViewModel → MainPage
        { "MainPageViewModel", "MainPage" },           // MainPageViewModel → MainPage (não MainPagePage)
        { "LocalBackupViewModel", "BackupConfigPage" }, // LocalBackupViewModel → BackupConfigPage
    };

    public object? Convert(object? value, Type targetType, object? parameter, 
        System.Globalization.CultureInfo culture)
    {
        if (value == null)
            return null;

        try
        {
            var viewModelType = value.GetType();
            var viewModelName = viewModelType.Name;

            System.Diagnostics.Debug.WriteLine($"[NAV] Tentando converter: {viewModelName}");

            // Verifica cache primeiro
            Type? pageType = null;
            if (_typeCache.TryGetValue(viewModelType, out var cachedType))
            {
                pageType = cachedType;
                System.Diagnostics.Debug.WriteLine($"[NAV] Cache hit para {viewModelName}");
            }
            else
            {
                // Determina o nome da página
                string pageName;
                
                // Primeiramente, verifica se há um mapeamento de exceção
                if (ExceptionMappings.TryGetValue(viewModelName, out var mappedName))
                {
                    pageName = mappedName;
                    System.Diagnostics.Debug.WriteLine($"[NAV] Usando mapeamento de exceção: {viewModelName} → {pageName}");
                }
                else
                {
                    // Convenção padrão: LoginViewModel → LoginPage, DizimistaListViewModel → DizimistaListPage
                    pageName = viewModelName.EndsWith("ViewModel") 
                        ? viewModelName.Replace("ViewModel", "Page")
                        : viewModelName; // Fallback caso não termine em ViewModel
                }

                System.Diagnostics.Debug.WriteLine($"[NAV] Nome da página esperada: {pageName}");

                // Procura o tipo da página na assembleia usando reflexão
                pageType = viewModelType.Assembly
                    .GetTypes()
                    .FirstOrDefault(t => t.Name == pageName);

                // Armazena em cache (pode ser null)
                _typeCache[viewModelType] = pageType;

                System.Diagnostics.Debug.WriteLine($"[NAV] Procurado na assembleia: {pageName} → {(pageType?.Name ?? "NÃO ENCONTRADO")}");
            }

            if (pageType == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Página não encontrada para {viewModelName}");
                return null;
            }

            // Instancia a página dinamicamente
            object? page = null;
            try
            {
                page = Activator.CreateInstance(pageType);
                System.Diagnostics.Debug.WriteLine($"[NAV] Página instanciada: {pageType.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Falha ao instanciar {pageType.Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
                return null;
            }

            // Define o ViewModel como DataContext da página
            if (page is Avalonia.Controls.Control control)
            {
                control.DataContext = value;
                System.Diagnostics.Debug.WriteLine($"[NAV] ✓ {viewModelName} → {pageType.Name}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AVISO] {pageType.Name} não é um Control. Tipo: {pageType.BaseType?.Name}");
            }

            return page;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Exceção geral no converter: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


