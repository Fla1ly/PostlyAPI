using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

public class UserDto
{
    public string userId { get; }
    public required string username { get; set; }
    public required string email { get; set; }
    public required string password { get; set; }

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

public class PostDto
{
    public string author { get; set; }
    public string postId { get; set; }
    public required string category { get; set; }
    public required string title { get; set; }
    public required string subtitle { get; set; }
    public required string description { get; set; }
    public required string status { get; set; }
    public required string visibility { get; set; }

    public PostDto()
    {
        postId = Guid.NewGuid().ToString();
    }

}
public class UpdatePostDto
{
    public string title { get; set; }
    public string subtitle { get; set; }
    public string description { get; set; }
    public string category { get; set; }
    public string visibility { get; set; }
}