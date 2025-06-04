using Microsoft.AspNetCore.Identity;

namespace Gargar.Common.Domain.Identity;

public class User : IdentityUser<Guid>
{
    //additional properties can be added here as needed
    //example
    /*
        [MaxLength(50)]
        public string? Firstname { get; set; }
        [MaxLength(50)]
        public string? Lastname { get; set; }
    */
}