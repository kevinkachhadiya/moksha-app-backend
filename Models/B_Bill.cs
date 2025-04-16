#nullable enable

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static MAPI.Models.B_Bill;

namespace MAPI.Models
{
    public class B_Bill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int B_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BillNo { get; set; } = null!; // Non-nullable, initialized with null-forgiving operator

        [Required]
        [MaxLength(100)]
        public string BuyerName { get; set; } = null!; // Non-nullable

        public List<B_BillItem> Items { get; set; } // Initialized in constructor

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalBillPrice { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public PaymentMethodType PaymentMethod { get; set; }

        public bool IsPaid { get; set; }

        public Boolean IsActive { get; set; }

        public B_Bill()
        {
            CreatedAt = DateTime.UtcNow;
            Items = new List<B_BillItem>(); // Initialize to prevent null
        }

        public enum PaymentMethodType
        {
            Cash,
            CreditCard,
            BankTransfer
        }
    }

    public class B_BillItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Navigation property (nullable if not always loaded)
        public Material? Material { get; set; }

        public decimal TotalPrice => Price * Quantity;

        public string? ColorName => Material?.ColorName; // Nullable return type
       

    }

    public class Create_B_Bill_Dto
    {
        public string BuyerName { get; set; } = "";
        public bool IsPaid { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public List<B_BillItemDto> Items { get; set; }
    }
    
   public class Edit_B_Bill_Dto
    {
        public int id { get; set; }
        public string BuyerName { get; set; } = "";
        public bool IsPaid { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public List<B_BillItemDto> Items { get; set; }
    }
    public class B_BillItemDto
    {
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class BillListViewModel
    {
        public IEnumerable<B_Bill>? Bills { get; set; }
        public string SearchTerm { get; set; } = "";
        public string SortColumn { get; set; } = "";
        public string SortDirection { get; set; } = "";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }

}