using Microsoft.Maui.Controls;

namespace Dizimo.Behaviors;

public class NumericValidationBehavior : Behavior<Entry>
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

        // Proteger contra NewTextValue nulo
        if (string.IsNullOrEmpty(e.NewTextValue)) return;

        // Remove caracteres não numéricos
        var newText = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
        
        if (e.NewTextValue != newText)
        {
            entry.Text = newText;
        }
    }
}
