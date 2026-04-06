using Microsoft.AspNetCore.Identity;

namespace SIPAD.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
