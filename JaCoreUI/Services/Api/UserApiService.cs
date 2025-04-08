namespace JaCoreUI.Services.Api;

public class UserApiService
{
    public Models.User.User GetAdmin()
    {
        return new Models.User.User
        {
            Username = "JaCore",
            Email = "admin@JaCore.com",
            Roles = ["Admin", "User"],
        };
    }
}