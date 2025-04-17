namespace Application.Common.Models;
public class UsersVm
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int AccessFailedCount { get; set; } 
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
}
