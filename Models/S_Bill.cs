using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MAPI.Models
{
    public class S_Bill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-increment primary key
        public int S_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string S_BillNo { get; set; }

        [Required]
        [MaxLength(100)]
        public string SellerName { get; set; }

        public List<S_BillItem> Items { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total_S_Price { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public PaymentMethodTypes PaymentMethod { get; set; }

        [Required]
        [NotMapped]
        public DateOnly DueDate { get; set; }
        [Required]
        public status Status { get; set; }

        [Required]
        public decimal RemainAmount { get; set; }

        public bool IsPaid { get; set; }


        public S_Bill()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public enum PaymentMethodTypes
        {
            Cash,
            BankTransfer
        }

        public enum status
        {
            Pending,
            OverDue,
            Completed
        }
    }

    // BillItem class
    public class S_BillItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-increment primary key
        public int ID { get; set; }

        [Required]
        public int P_Stock_Id { get; set; }

        [Required]
        public int St_Bags { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]  // Weight per bag
        public decimal St_Weight { get; set; }

        // Calculated property for total weight
        [Column(TypeName = "decimal(18,2)")]
        public decimal S_totalWeight => St_Bags * St_Weight;

        [Required]
        public decimal price { get; set; }
        public Stock Stock { get; set; }


    }
}
