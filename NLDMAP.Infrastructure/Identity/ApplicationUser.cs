using Microsoft.AspNetCore.Identity;


namespace NLDMAP.Infrastructure.Identity

{
    public sealed class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
