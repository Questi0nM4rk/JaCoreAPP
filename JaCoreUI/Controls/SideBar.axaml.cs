using Avalonia;
using Avalonia.Controls;

namespace JaCoreUI.Controls;

public class SideBar : ListBox
{
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<SideBar, bool>(
        nameof(IsExpanded));

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
}