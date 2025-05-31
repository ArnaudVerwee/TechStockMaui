// Models/User.cs
namespace TechStockMaui.Models
{
    public class User
    {
        public string UserName { get; set; } = string.Empty;  // ✅ Correspond à l'API
        public List<string> Roles { get; set; } = new();     // ✅ Correspond à l'API

        // Propriétés calculées pour compatibilité
        public string Id => UserName;           // ✅ Pour compatibilité
        public string Name => UserName;         // ✅ Pour compatibilité
        public string Email => UserName;        // ✅ Pour compatibilité (UserName contient l'email)
    }
}