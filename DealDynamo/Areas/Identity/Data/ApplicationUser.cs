using DealDynamo.Models;
using Microsoft.AspNetCore.Identity;

namespace DealDynamo.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public bool IsAdmin { get; set; } = false;
    public bool IsSeller { get; set; } = false;
    public bool IsBuyer { get; set; } = false;
}

