using DealDynamo.Models;
using Microsoft.AspNetCore.Identity;

namespace DealDynamo.Areas.Identity.Data;

public class ApplicationUser : IdentityUser
{
    public bool IsAdmin { get; set; } = false;
    public bool IsSeller { get; set; } = false;
    public bool IsBuyer { get; set; } = false;
}

