using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Interactivity;

namespace Dizimo.Behaviors;

/// <summary>
///     Behavior genérico que sincroniza CheckBox com uma coleção de selecionados
///     Funciona com qualquer tipo de entidade (Dizimista, Usuario, Oferta, etc)
/// </summary>
public class CheckBoxSelectionBehavior : AvaloniaObject
{
    public static readonly AttachedProperty<object?> ItemProperty =
        AvaloniaProperty.RegisterAttached<CheckBoxSelectionBehavior, Control, object?>("Item");

    public static readonly AttachedProperty<IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterAttached<CheckBoxSelectionBehavior, Control, IList?>("SelectedItems");

    private static readonly Dictionary<CheckBox, EventHandler<RoutedEventArgs>> _checkBoxHandlers = new();
    private static readonly Dictionary<CheckBox, NotifyCollectionChangedEventHandler> _collectionHandlers = new();

    static CheckBoxSelectionBehavior()
    {
        ItemProperty.Changed.AddClassHandler<CheckBox>((cb, e) => OnItemChanged(cb, e));
        SelectedItemsProperty.Changed.AddClassHandler<CheckBox>((cb, e) => OnSelectedItemsChanged(cb, e));
    }

    public static object? GetItem(Control control)
    {
        return control.GetValue(ItemProperty);
    }

    public static void SetItem(Control control, object? value)
    {
        control.SetValue(ItemProperty, value);
    }

    public static IList? GetSelectedItems(Control control)
    {
        return control.GetValue(SelectedItemsProperty);
    }

    public static void SetSelectedItems(Control control, IList? value)
    {
        control.SetValue(SelectedItemsProperty, value);
    }

    private static void OnItemChanged(CheckBox cb, AvaloniaPropertyChangedEventArgs e)
    {
        UpdateBinding(cb);
    }

    private static void OnSelectedItemsChanged(CheckBox cb, AvaloniaPropertyChangedEventArgs e)
    {
        var oldCollection = e.OldValue as INotifyCollectionChanged;
        var newCollection = e.NewValue as INotifyCollectionChanged;

        // Remover handler antigo
        if (oldCollection != null && _collectionHandlers.TryGetValue(cb, out var oldHandler))
        {
            oldCollection.CollectionChanged -= oldHandler;
            _collectionHandlers.Remove(cb);
        }

        // Adicionar handler novo
        if (newCollection != null)
        {
            NotifyCollectionChangedEventHandler handler = (s, args) => OnCollectionChanged(cb);
            newCollection.CollectionChanged += handler;
            _collectionHandlers[cb] = handler;
        }

        // Adicionar handler para mudanças no CheckBox se ainda não existir
        if (!_checkBoxHandlers.ContainsKey(cb))
        {
            EventHandler<RoutedEventArgs> checkBoxHandler = (s, args) => OnCheckBoxChanged(cb);
            cb.IsCheckedChanged += checkBoxHandler;
            _checkBoxHandlers[cb] = checkBoxHandler;
        }

        Debug.WriteLine("[CheckBoxSync] Coleção configurada para CheckBox");
        UpdateBinding(cb);
    }

    private static void OnCollectionChanged(CheckBox cb)
    {
        Debug.WriteLine("[CheckBoxSync] Coleção foi alterada, atualizando CheckBox");
        UpdateBinding(cb);
    }

    private static void OnCheckBoxChanged(CheckBox cb)
    {
        var item = GetItem(cb);
        var selecionados = GetSelectedItems(cb);

        if (item == null || selecionados == null)
        {
            Debug.WriteLine("[CheckBoxSync] Item ou coleção é null");
            return;
        }

        Debug.WriteLine($"[CheckBoxSync] CheckBox mudou para {cb.IsChecked}");

        if (cb.IsChecked == true)
        {
            if (!selecionados.Contains(item))
            {
                selecionados.Add(item);
                Debug.WriteLine("[CheckBoxSync] Item adicionado à coleção");
            }
        }
        else if (cb.IsChecked == false)
        {
            if (selecionados.Contains(item))
            {
                selecionados.Remove(item);
                Debug.WriteLine("[CheckBoxSync] Item removido da coleção");
            }
        }
    }

    private static void UpdateBinding(CheckBox cb)
    {
        var item = GetItem(cb);
        var selecionados = GetSelectedItems(cb);

        if (item != null && selecionados != null)
        {
            var isSelected = selecionados.Contains(item);
            if (cb.IsChecked != isSelected)
            {
                cb.IsChecked = isSelected;
                Debug.WriteLine(
                    $"[CheckBoxSync] Estado visual atualizado - IsChecked: {cb.IsChecked}, Count na coleção: {selecionados.Count}");
            }
        }
    }
}