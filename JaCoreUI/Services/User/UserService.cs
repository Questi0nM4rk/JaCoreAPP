using System.Collections.Generic;
using JaCoreUI.Services.Api;

namespace JaCoreUI.Services.User;

public class UserService(UserApiService userApiService)
{
    public Models.User.User CurrentUser { get; set; } = userApiService.GetAdmin();
}