using System.Collections.Generic;

namespace JaCoreUI.Models.User;

public class User
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    
    public required List<string> Roles { get; set; }
    
    public string DisplayName => !string.IsNullOrEmpty(Username) ? Username : Email;
}