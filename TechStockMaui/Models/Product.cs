namespace TechStockMaui.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }

        public int TypeId { get; set; }
        public string? TypeName { get; set; } 

        public int SupplierId { get; set; }
        public string? SupplierName { get; set; } 
    }
}
