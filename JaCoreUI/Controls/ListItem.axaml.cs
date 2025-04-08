using Avalonia;
using Avalonia.Controls;
using JaCoreUI.Data;

namespace JaCoreUI.Controls;

public class ListItem : ListBoxItem
{
    public ListItem()
    {
    }

    public ListItem(object? content, string iconText, ApplicationPageNames applicationPageName)
    {
        Content = content;
        IconText = iconText;
        ParentPage = applicationPageName;
    }

    public static readonly StyledProperty<string> IconTextProperty = AvaloniaProperty.Register<ListItem, string>(
        nameof(IconText));

    public string IconText
    {
        get => GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly StyledProperty<ApplicationPageNames> ParentPageProperty =
        AvaloniaProperty.Register<ListItem, ApplicationPageNames>(
            nameof(IconText));

    public ApplicationPageNames ParentPage
    {
        get => GetValue(ParentPageProperty);
        set => SetValue(ParentPageProperty, value);
    }
}