using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MAPI.Models
{
    // Material class
    public class Material
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-increment in SQL Server
        public int Id { get; set; }  // Primary Key

        [Required]
       
        public string ColorName { get; set; }  // Color of the material

        [Required]
        public decimal BasePrice { get; set; }  // Base price of the material
    }

  
    
}
