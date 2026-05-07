using Avalonia.Controls;
using Avalonia.Input;

namespace Dizimo.Behaviors;

public class WhatsAppFormattingBehavior
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

        // Limitar a 11 dígitos
        cleanedText = cleanedText.Length > 11 ? cleanedText.Substring(0, 11) : cleanedText;

        // Formatar: (99) 99999-9999
        var formattedText = cleanedText;
        
        if (cleanedText.Length > 0)
        {
            if (cleanedText.Length <= 2)
            {
                formattedText = $"({cleanedText}";
            }
            else if (cleanedText.Length <= 7)
            {
                formattedText = $"({cleanedText.Substring(0, 2)}) {cleanedText.Substring(2)}";
            }
            else
            {
                formattedText = $"({cleanedText.Substring(0, 2)}) {cleanedText.Substring(2, 5)}-{cleanedText.Substring(7)}";
            }
        }

        if (textBox.Text != formattedText)
        {
            textBox.Text = formattedText;
        }
    }
}



