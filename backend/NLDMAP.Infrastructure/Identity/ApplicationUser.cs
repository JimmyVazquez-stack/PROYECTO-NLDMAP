using Microsoft.AspNetCore.Identity;

namespace NLDMAP.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}
