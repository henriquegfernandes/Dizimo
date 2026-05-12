using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dizimo.ViewModels;

namespace Dizimo.Pages;

/// <summary>
///     Página de login - Otimizada para Avalonia UI
/// </summary>
public class LoginPage : UserControl
{
    public LoginPage()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] LoginPage inicializado");
    }

    /// <summary>
    ///     Handler para tratar Enter no login - move foco para senha
    /// </summary>
    public void OnLoginKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            FindSenhaTextBox(this);
            e.Handled = true;
        }
    }

    /// <summary>
    ///     Handler para tratar Enter na senha - executa comando de login
    /// </summary>
    public void OnSenhaKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return && DataContext is LoginViewModel vm)
            if (vm.LoginCommand.CanExecute(null))
            {
                vm.LoginCommand.Execute(null);
                e.Handled = true;
            }
    }

    /// <summary>
    ///     Reset do estado de login quando o DataContext muda
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is LoginViewModel vm) vm.ResetLoginState();
    }

    /// <summary>
    ///     Procura pelo TextBox de senha na hierarquia
    /// </summary>
    private void FindSenhaTextBox(Control parent)
    {
        if (parent is TextBox textBox && textBox.Name == "SenhaTextBox")
        {
            textBox.Focus();
            return;
        }

        if (parent is Panel panel)
            foreach (var child in panel.Children)
                if (child is Control control)
                    FindSenhaTextBox(control);
    }
}