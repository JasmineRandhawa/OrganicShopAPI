namespace OrganicShopAPI.Models
{
    public record AppUser(int Id, string Name,string Email, bool IsAdmin)
    {
    }
}