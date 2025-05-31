using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TechStockMaui.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "SerialNumber")]
        public string SerialNumber { get; set; } = string.Empty;

        [Display(Name = "Item Types")]
        [ForeignKey("TypeArticle")]
        public int TypeId { get; set; }

        // Ces propriétés ne seront pas sérialisées (pour éviter les références circulaires)
        [JsonIgnore]
        public TypeArticle.TypeArticle? TypeArticle { get; set; }

        [Display(Name = "Supplier")]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        // Ces propriétés ne seront pas sérialisées (pour éviter les références circulaires)
        [JsonIgnore]
        public Supplier.Supplier? Supplier { get; set; }

        [Display(Name = "Assigned User")]
        public int? AssignedUserId { get; set; }

        // ✅ CES PROPRIÉTÉS VIENNENT DIRECTEMENT DE L'API (plus de doublons)
        public string? TypeName { get; set; }
        public string? SupplierName { get; set; }
        public string? AssignedUserName { get; set; }
    }
}