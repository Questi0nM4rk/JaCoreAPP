using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Data;

namespace JaCoreUI.ViewModels.Settings;

public partial class SettingsViewModel() : PageViewModel(ApplicationPageNames.Settings, ApplicationPageNames.Settings)
{
    protected override void OnDesignTimeConstructor()
    {
        throw new System.NotImplementedException();
    }

    public override bool Validate()
    {
        throw new System.NotImplementedException();
    }
}