// Models/User.cs
namespace TechStockMaui.Models
{
    public class User
    {
        public string UserName { get; set; } = string.Empty;  
        public List<string> Roles { get; set; } = new();     

        
        public string Id => UserName;           
        public string Name => UserName;         
        public string Email => UserName;        
    }
}