using MAPI.Migrations;
using MAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Globalization;
using static MAPI.Models.Party;

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
        public async Task<IActionResult> CreateParty([FromBody] Party p)
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


        [HttpPut("EditParty")]
        public async Task<IActionResult> EditParty([FromBody] Party p)
        {

            var record = await _context.Party.FirstOrDefaultAsync(ph => ph.P_number == p.P_number && ph.p_t == p.p_t && ph.Id != p.Id);

            if (record != null)
            {
                return BadRequest("Phonenumber is already registered in " + p.p_t.ToString() + " section");
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
                var existingParty = await _context.Party.FindAsync(p.Id);
                if (existingParty != null)
                {
                    existingParty.P_Name = p.P_Name;
                    existingParty.P_number = p.P_number;
                    existingParty.p_t = p.p_t;
                    existingParty.P_Address = p.P_Address;
                    existingParty.IsActive = p.IsActive;

                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, id = p.Id });
                }
                else
                {
                    return BadRequest("Problem is accure while fetching data on the " + p.p_t.ToString() + " section");
                  

                }
                
               
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

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteParty(int id)
        {
            var record = await _context.Party.Where(p=>p.IsActive == true).FirstOrDefaultAsync(p1=>p1.Id == id);
            if (record != null)
            {
                record.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok($"Party deleted successfully from the {record.p_t} section.");


            }
            else 
            {
                return NotFound(new { message = "Record not found for the given ID." });

            }

        }
        
        [HttpGet("SupplierSearch")]
        public async Task<IActionResult> SupplierSearch([FromQuery]string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return BadRequest("Only White Space are not allowed");
            }

            // Filter based on the search term (case-insensitive)
            var filteredSuppliers = _context.Party
               .Where(s => s.P_Name.ToLower().Contains(search.ToLower()) && s.p_t == P_t.Supplier && s.IsActive == true)
                .Take(10) // Limit results to prevent overwhelming the UI
                .Select(s => new
                {
                    P_Name = s.P_Name,
                    P_number = s.P_number
                })
                .ToList();

            if (filteredSuppliers.Count() >= 1)
           {
                return Ok(filteredSuppliers);

            }
            else
            {
                return BadRequest("Zero record found");

            }

        }

        [HttpGet("AllParty")]

        public async Task<IActionResult> AllParty(
               [FromQuery] string party = "All",
               [FromQuery] string searchTerm = "",
               [FromQuery] string sortColumn = "P_Name",
               [FromQuery] string sortDirection = "asc",
               [FromQuery] int page = 1,
               [FromQuery] int pageSize = 10)
        {

            IQueryable<Party> query;
            

            if (party != "All")
            {

                var result = _context.Party
                  .Where(p => p.IsActive)
                  .AsEnumerable() // Forces in-memory evaluation
                  .Where(p => p.p_t.ToString() == party)
                  .ToList();

                query = result.AsQueryable();
            }
            else
            {
                query = _context.Party.AsQueryable().Where(b => b.IsActive == true);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {

                  query = query
                         .Where(b =>
                              b.P_Name.ToLower().Contains(searchTerm.ToLower()) ||
                              b.P_number.ToString().ToLower().Contains(searchTerm.ToLower()) ||
                              b.P_Address.ToLower().ToString().Contains(searchTerm.ToLower()));
                              }

          

            switch (sortColumn)
            {
                case "P_Name":
                    query = sortDirection == "asc"
                        ? query.OrderBy(b => b.P_Name)
                        : query.OrderByDescending(b => b.P_Name);
                    break;
                case "P_number":
                    query = sortDirection == "asc"
                        ? query.OrderBy(b => b.P_number)
                        : query.OrderByDescending(b => b.P_number);
                    break; 
                case "P_Address":
                    query = sortDirection == "asc"
                        ? query.OrderBy(b => b.P_Address)
                        : query.OrderByDescending(b => b.P_Address);
                    break;

                default: 
                    query = sortDirection == "asc"
                        ? query.OrderBy(b => b.P_Name)
                        : query.OrderByDescending(b => b.P_Name);
                    break;
            }

            // Pagination
             int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var Parties_ = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var parties = new
            {
                Party = Parties_,
                party_ = party,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
            return Ok(parties);
        }
    
    }
}
