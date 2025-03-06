// Controls/UserButton.axaml.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Svg.Skia;

namespace JaCoreUI.Controls
{
    public class UserButton : Button
    {
        public static readonly StyledProperty<SvgSource> IconSourceProperty =
            AvaloniaProperty.Register<UserButton, SvgSource>(nameof(IconSource));
            
        public static readonly StyledProperty<string> UserTextProperty =
            AvaloniaProperty.Register<UserButton, string>(nameof(UserText));

        public SvgSource IconSource
        {
            get => GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public string UserText
        {
            get => GetValue(UserTextProperty);
            set => SetValue(UserTextProperty, value);
        }
    }
}