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
        public User User { get; set; } = null!;

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [ForeignKey("State")]
        public int StateId { get; set; }
        public States State { get; set; } = null!;

        public string Signature { get; set; } = string.Empty;
        public DateTime AssignmentDate { get; set; }
        public DateTime SignatureDate { get; set; }

        // Propriétés calculées pour le binding XAML
        [NotMapped]
        public bool IsSignaturePending => string.IsNullOrEmpty(Signature);

        [NotMapped]
        public bool IsSignatureValid => !string.IsNullOrEmpty(Signature);

        [NotMapped]
        public DateTime? SignatureDateNullable => SignatureDate == default(DateTime) ? null : SignatureDate;
    }
}