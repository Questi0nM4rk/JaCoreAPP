using Avalonia;
using Avalonia.Controls;

namespace JaCoreUI.Controls;

public class ListItem : ListBoxItem
{
    public static readonly StyledProperty<string> IconTextProperty = AvaloniaProperty.Register<ListItem, string>(
        nameof(IconText));

    public string IconText
    {
        get => GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }
}