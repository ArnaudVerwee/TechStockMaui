using System.ComponentModel.DataAnnotations;

namespace TechStockMaui.Models.Supplier
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
