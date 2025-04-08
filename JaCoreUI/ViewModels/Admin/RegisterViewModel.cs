using JaCoreUI.Data;

namespace JaCoreUI.ViewModels.Admin;

public class RegisterViewModel() : PageViewModel(ApplicationPageNames.Register, ApplicationPageNames.Dashboard)
{
    protected override void OnDesignTimeConstructor()
    {
    }

    public override bool Validate()
    {
        throw new System.NotImplementedException();
    }
}