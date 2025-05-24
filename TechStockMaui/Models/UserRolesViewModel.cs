namespace TechStockMaui.Models
{
    public class UserRolesViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();

        public string RolesAsString => string.Join(", ", Roles);
    }
}
