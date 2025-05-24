using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TechStockMaui.Models
{
    public class MaterialManagement
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!; // Adapte le nom de la classe User selon ton projet MAUI

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [ForeignKey("State")]
        public int StateId { get; set; }
        public States State { get; set; } = null!;

        public string Signature { get; set; } = string.Empty;
        public DateTime AssignmentDate { get; set; }
        public DateTime SignatureDate { get; set; }
    }
}
