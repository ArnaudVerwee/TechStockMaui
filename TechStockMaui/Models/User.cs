// Models/User.cs
namespace TechStockMaui.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;      // si l’API le fournit
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;   // optionnel
    }
}
