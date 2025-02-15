#nullable enable  // Enable nullable reference types

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MAPI.Models
{
    public class Stock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StockId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        public int TotalBags { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Weight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalWeight => TotalBags * Weight;

        [Column(TypeName = "decimal(18,2)")]  // Add this to AvailableStock
        public decimal AvailableStock { get; set; }


        // Change to non-nullable if relationship is required
        public Material Material { get; set; } = null!;  // Remove '?' and initialize
        

        public void RemoveStock(decimal quantitySold)
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

        public void AddStock(int bagsAdded, decimal weightPerBag)
        {
            if (weightPerBag <= 0)
            {
                throw new ArgumentException("Weight per bag must be greater than zero.");
            }

            Weight = weightPerBag;
            TotalBags += bagsAdded;  // Changed to += instead of =
            AvailableStock += bagsAdded * weightPerBag;
        }
    }
}