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
        public decimal BasePrice { get; set; }
        
        [Required]
        public bool IsActive { get; set; }
    }

    public class MaterialsListViewModel
    {
        public IEnumerable<Material> Materials { get; set; }
        public string SearchTerm { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }
}