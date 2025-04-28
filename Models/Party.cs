using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAPI.Models
{
    public class Party
    {

        
        
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public string P_Name { get; set; } = string.Empty;

            public string P_number { get; set; } = string.Empty;

            public bool IsActive { get; set; } = true;
            public P_t p_t { get; set; }
            public enum P_t
            {
            
                Supplier,

                Customer
            }
            public string P_Address { get; set; } = string.Empty;

       
    }
}