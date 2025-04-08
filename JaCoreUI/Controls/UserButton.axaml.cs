// Controls/UserButton.axaml.cs

using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace JaCoreUI.Controls;

public class UserButton : Button
{
    public static readonly StyledProperty<string> IconSourceProperty =
        AvaloniaProperty.Register<UserButton, string>(nameof(IconSource));


    public string IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }
}