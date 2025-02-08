using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MAPI.Models
{
    public class Stock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-increment primary key
        public int StockId { get; set; }  // Primary Key for stock record

        [Required]
        public int MaterialId { get; set; }  // Foreign Key to Material

        [Required]
        public int TotalBags { get; set; }  // Total number of bags in stock

        [Required]
        [Column(TypeName = "decimal(18,2)")]  // Weight per bag
        public decimal Weight { get; set; }

        // Calculated property for total weight
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalWeight => TotalBags * Weight;  // Total weight = TotalBags * Weight per bag

        // Available stock after purchases or sales
        [Column(TypeName = "decimal(18,2)")]
        public decimal AvailableStock { get; set; }  // Track how much stock is available

        // Navigation property to Material
        public Material Material { get; set; }


        // Method to update available stock (e.g., after a sale)
        public void removeStock(decimal quantitySold)
        {
            if (quantitySold <= AvailableStock)
            {
                AvailableStock -= quantitySold;
            }
            else
            {
                throw new InvalidOperationException("Not enough stock available.");
            }
        }

        public void AddStock(int bagsAdded, decimal Weightperbag)
        {
            // Validate if the weight per bag is a positive number
            if (Weightperbag <= 0)
            {
                throw new ArgumentException("Weight per bag must be greater than zero.");
            }

            // Update the weight per bag for this stock entry
            Weight = Weightperbag;

            // Update the total number of bags in stock
            TotalBags = bagsAdded;

            // Update the AvailableStock based on the weight of the new bags
            AvailableStock += TotalWeight;
        }
    }
}
