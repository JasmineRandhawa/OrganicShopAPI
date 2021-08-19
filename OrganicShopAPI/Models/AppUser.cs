namespace OrganicShopAPI.Models
{
    public class AppUser
    {
        public string AppUserId { get; set; }

        public string AppUserName { get; set; }

        public string Email { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsActive { get; set; }
    }
}