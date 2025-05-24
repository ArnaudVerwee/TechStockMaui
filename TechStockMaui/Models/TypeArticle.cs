using System.ComponentModel.DataAnnotations;
namespace TechStockMaui.Models.TypeArticle
{
    public class TypeArticle
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
