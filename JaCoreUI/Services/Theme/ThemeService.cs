using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JaCoreUI.Services.Theme;

public partial class ThemeService : ObservableObject
{
    [ObservableProperty] public partial string ThemeButtonText { get; set; } = "☀️ Light Mode";

    [RelayCommand]
    private void ToggleTheme()
    {
        Application.Current!.RequestedThemeVariant =
            Application.Current.RequestedThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;

        UpdateThemeButtonText();
    }

    private void UpdateThemeButtonText()
    {
        ThemeButtonText = Application.Current!.RequestedThemeVariant == ThemeVariant.Dark
            ? "☀️ Light Mode"
            : "🌙 Dark Mode";
    }
}