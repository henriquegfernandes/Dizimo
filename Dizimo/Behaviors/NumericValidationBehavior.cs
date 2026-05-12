using Avalonia.Input;

namespace Dizimo.Behaviors;

public class NumericValidationBehavior
{
    public static void Attach(TextBox textBox)
    {
        textBox.KeyDown += OnKeyDown;
    }

    public static void Detach(TextBox textBox)
    {
        textBox.KeyDown -= OnKeyDown;
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox) return;

        // Permitir apenas números e teclas de controle
        if (char.IsDigit((char)e.Key) || IsControlKey(e.Key)) return;

        e.Handled = true;
    }

    private static bool IsControlKey(Key key)
    {
        return key == Key.Back || key == Key.Delete || key == Key.Tab ||
               key == Key.Left || key == Key.Right || key == Key.Home || key == Key.End;
    }
}