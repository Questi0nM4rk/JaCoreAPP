using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace JaCoreUI.Controls;

public class SeparatorLabel : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<SeparatorLabel, string>(nameof(Text));

    public static readonly StyledProperty<double> LineThicknessProperty =
        AvaloniaProperty.Register<SeparatorLabel, double>(nameof(LineThickness), 1.0);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double LineThickness
    {
        get => GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }
}