using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static MAPI.Models.B_Bill;

namespace MAPI.Models
{
    public class C_Bill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(20)]
        public string GstBillNo { get; set; }

        [MaxLength(20)]
        public string NonGstBillNo { get; set; }

        [Required]
        [MaxLength(15)]
        public string CustomerGSTIN { get; set; }

        [Required]
        public int PlaceOfSupply { get; set; }

        [Required]
        public decimal SumValue { get; set; }

        [Required]
        public decimal TotalWithGstValue { get; set; }

        public decimal GstAmount { get; set; }

        [Required]
        public bool IncludeGst { get; set; }

        [Required]
        public decimal AmountPaid { get; set; }

        [Required]
        public decimal BalanceDue { get; set; }

        [Required]
        public DateTime BillDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }

        public decimal CgstPercent { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal GstPercent { get; set; }

        [MaxLength(20)]
        public string HsnCode { get; set; }

        public List<C_BillItem> StockItems { get; set; }

        public string GetBillNumber() => IncludeGst ? GstBillNo : NonGstBillNo;
    }
    public class C_BillItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [ForeignKey("StockId")]
        public int StockId { get; set; }

        [Required]
        public int Bags { get; set; }

        [Required]
        public decimal TotalWeight { get; set; }

        [Required]
        public decimal PricePerKg { get; set; }

        public decimal ItemAmount { get; set; }

        public decimal Extra { get; set; }

    }
}
