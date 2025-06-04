using Microsoft.AspNetCore.Identity;

namespace Gargar.Common.Domain.Identity;

public class Role : IdentityRole<Guid>
{
    //additional properties can be added here as needed
    //example
    /*
        [MaxLength(50)]
        public string? Description { get; set; }
    */
}