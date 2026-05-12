using Avalonia.Input;

namespace Dizimo.Behaviors;

public class CepFormattingBehavior
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
        if (sender is not TextBox textBox) return;

        // Remove caracteres não numéricos
        var cleanedText = new string((textBox.Text ?? "").Where(char.IsDigit).ToArray());

        // Limitar a 8 dígitos
        cleanedText = cleanedText.Length > 8 ? cleanedText.Substring(0, 8) : cleanedText;

        // Formatar: 99999-999
        var formattedText = cleanedText;

        if (cleanedText.Length > 0)
        {
            if (cleanedText.Length <= 5)
                formattedText = cleanedText;
            else
                formattedText = $"{cleanedText.Substring(0, 5)}-{cleanedText.Substring(5)}";
        }

        if (textBox.Text != formattedText) textBox.Text = formattedText;
    }
}