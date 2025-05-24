using System.ComponentModel.DataAnnotations;

namespace TechStockMaui.Models
{
    public class States
    {
        [Key]
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
