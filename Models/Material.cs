using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;  // Required for Index attribute

namespace MAPI.Models
{
    [Index(nameof(ColorName), IsUnique = true)]  // Creates unique constraint in database
    public class Material
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]  // Added length limit
        public string ColorName { get; set; } = null!;  // Non-nullable with initialization

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }
    
    
    }
}