using Microsoft.Maui.Controls;

namespace Dizimo.Behaviors;

public class CepFormattingBehavior : Behavior<Entry>
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

        // Limitar a 8 dígitos
        cleanedText = cleanedText.Length > 8 ? cleanedText.Substring(0, 8) : cleanedText;

        // Formatar: 99999-999
        var formattedText = cleanedText;
        
        if (cleanedText.Length > 0)
        {
            if (cleanedText.Length <= 5)
            {
                formattedText = cleanedText;
            }
            else
            {
                formattedText = $"{cleanedText.Substring(0, 5)}-{cleanedText.Substring(5)}";
            }
        }

        if (entry.Text != formattedText)
        {
            entry.Text = formattedText;
        }
    }
}
