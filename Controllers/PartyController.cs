using MAPI.Migrations;
using MAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartyController : Controller
    {
        private readonly AppDbContext _context;
        public PartyController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("CreateParty")]
        public async Task<IActionResult> CreateParty([FromBody] Party p) // Note: "Party" not "party"  
        {
            var record = await _context.Party.FirstOrDefaultAsync(ph => ph.P_number == p.P_number && ph.p_t == p.p_t );

            if (record!=null)
            {
                return BadRequest("Phonenumber is already registered in " + p.p_t.ToString()  +" section");
            }

            // Validate all fields  
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(p.P_Name))
                errors.Add("Party name is required");
            else if (p.P_Name.Length < 3)
                errors.Add("Party name must be at least 3 characters");

            if (string.IsNullOrWhiteSpace(p.P_number))
                errors.Add("Phone number is required");
            else if (p.P_number.Length != 10 || !p.P_number.All(char.IsDigit))
                errors.Add("Phone number must be exactly 10 digits");

            if (!Enum.IsDefined(typeof(Party.P_t), p.p_t))
                errors.Add("Invalid party type selected");

            if (!p.IsActive)
                errors.Add("Party must be active");

            if (errors.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors
                });
            }


            try
            {
                await _context.Party.AddAsync(p); // Proper async method  
                await _context.SaveChangesAsync(); // Proper async method  
                return Ok(new { success = true, id = p.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error saving to database",
                    error = ex.Message
                });
            }
        }
    }
}
