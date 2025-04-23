#nullable enable  // Enable nullable reference types

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;
using MAPI.Models;

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


        [JsonIgnore]
        [ValidateNever]
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
        public bool isActive { get; set; }
    }
}
public class Stock_
{
    public int StockId { get; set; }
    public int MaterialId { get; set; }
    public string ColorName { get; set; }
    public int TotalBags { get; set; }
    public decimal Weight { get; set; }
    public decimal TotalWeight => TotalBags * Weight;
    public decimal AvailableStock { get; set; }
    public bool isActive { get; set; }
}
public class stockListViewModel
{
    public IEnumerable<Stock_> Stock { get; set; }
    public string SearchTerm { get; set; }
    public string SortColumn { get; set; }
    public string SortDirection { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}

