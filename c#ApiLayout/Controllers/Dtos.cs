using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

public class UserDto
{
    public string userId { get; }
    public required string username { get; set; }
    public required string email { get; set; }
    public string password { get; set; }

    public UserDto()
    {
        userId = Guid.NewGuid().ToString();
    }
}

public class LoginDto
{
    public string username { get; set; }
    public string password { get; set; }
}