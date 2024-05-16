using System.ComponentModel.DataAnnotations;

namespace DealDynamo.Models.UserViewModels
{
    public class UserEditViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsSeller { get; set; } = false;
        public bool IsBuyer { get; set; } = false;
    }
}
