using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace JaCoreUI.Models.UI;

public static class ErrorDialog
{
    /// <summary>
    /// Displays an error message in a popup dialog
    /// </summary>
    /// <param name="errorMessage">The error message to display</param>
    /// <param name="title">Optional custom title (defaults to "Error")</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<ButtonResult> ShowWithButtonsAsync(
        string message, 
        string title = "Warning", 
        ButtonEnum buttons = ButtonEnum.YesNo, 
        Icon icon = Icon.Warning)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ContentTitle = title,
                ContentMessage = message,
                ButtonDefinitions = buttons,
                Icon = icon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            });

        return await messageBox.ShowWindowAsync();
    }
 
}