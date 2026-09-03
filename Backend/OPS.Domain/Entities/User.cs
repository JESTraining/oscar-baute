using OPS.Domain.Common;

namespace OPS.Domain.Entities;

public class User :  BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.RegularUser;

}