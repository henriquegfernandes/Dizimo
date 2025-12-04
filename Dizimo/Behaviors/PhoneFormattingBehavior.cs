using Microsoft.Maui.Controls;

namespace Dizimo.Behaviors;

public class PhoneFormattingBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry entry)
    {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry) return;

        // Remove caracteres não numéricos
        var cleanedText = new string(e.NewTextValue.Where(char.IsDigit).ToArray());

        // Limitar a 11 dígitos
        cleanedText = cleanedText.Length > 11 ? cleanedText.Substring(0, 11) : cleanedText;

        // Formatar: (99) 9999-9999 ou (99) 99999-9999
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
                // Verificar se tem 10 ou 11 dígitos para determinar o formato
                if (cleanedText.Length == 10)
                {
                    formattedText = $"({cleanedText.Substring(0, 2)}) {cleanedText.Substring(2, 4)}-{cleanedText.Substring(6)}";
                }
                else if (cleanedText.Length == 11)
                {
                    formattedText = $"({cleanedText.Substring(0, 2)}) {cleanedText.Substring(2, 5)}-{cleanedText.Substring(7)}";
                }
            }
        }

        if (entry.Text != formattedText)
        {
            entry.Text = formattedText;
        }
    }
}
