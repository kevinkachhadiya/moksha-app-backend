using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MAPI.Models
{
    public class B_Bill
    {
        
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-increment primary key
            public int B_Id { get; set; }

            [Required]
            [MaxLength(50)]
            public string BillNo { get; set; }

            [Required]
            [MaxLength(100)]
            public string BuyerName { get; set; }

            public List<B_BillItem> Items { get; set; }

            [Column(TypeName = "decimal(10,2)")]
            public decimal TotalBillPrice { get; set; }

            [Required]
            public DateTime CreatedAt { get; set; }

            [Required]
            public PaymentMethodType PaymentMethod { get; set; }

            public bool IsPaid { get; set; }

            public B_Bill()
            {
                CreatedAt = DateTime.UtcNow;
            }

            public enum PaymentMethodType
            {
                Cash,
                CreditCard,
                BankTransfer
            }
        }

        // BillItem class
        public class B_BillItem
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-increment primary key
            public int Id { get; set; }

            [Required]
            public int MaterialId { get; set; }  // Foreign Key to Material

            [Required]
            public int Quantity { get; set; }  // Quantity of material purchased

            [Required]
            [Column(TypeName = "decimal(18,2)")]  // Decimal type for price with precision
            public decimal Price { get; set; }  // Price at which the material is purchased

            // Navigation property to Material
            public Material Material { get; set; }

            // Calculating Total Price for the item
            public decimal TotalPrice => Price * Quantity;

            // Fetch the ColorName from Material for each item
            public string ColorName => Material?.ColorName;

        
    }
}
