using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dizimo.ViewModels;

namespace Dizimo.Pages;

/// <summary>
///     Página de configuração inicial - Criação do primeiro usuário
/// </summary>
public class SetupPage : UserControl
{
    public SetupPage()
    {
        AvaloniaXamlLoader.Load(this);
        Debug.WriteLine("[INFO] SetupPage inicializado");
    }

    /// <summary>
    ///     Handler para tratar Enter na senha - move foco para confirmação
    /// </summary>
    public void OnSenhaKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            FindSenhaConfirmacaoTextBox(this);
            e.Handled = true;
        }
    }

    /// <summary>
    ///     Handler para tratar Enter na confirmação - executa o comando
    /// </summary>
    public void OnSenhaConfirmacaoKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return && DataContext is SetupViewModel vm)
            if (vm.CriarPrimeiroUsuarioCommand.CanExecute(null))
            {
                vm.CriarPrimeiroUsuarioCommand.Execute(null);
                e.Handled = true;
            }
    }

    /// <summary>
    ///     Procura pelo TextBox de confirmação de senha na hierarquia
    /// </summary>
    private void FindSenhaConfirmacaoTextBox(Control parent)
    {
        if (parent is TextBox textBox && textBox.Name == "SenhaConfirmacaoTextBox")
        {
            textBox.Focus();
            return;
        }

        if (parent is Panel panel)
            foreach (var child in panel.Children)
                if (child is Control control)
                    FindSenhaConfirmacaoTextBox(control);
    }
}