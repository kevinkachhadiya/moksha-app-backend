using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace MAPI.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaterialsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Materials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetMaterials([FromQuery] string searchTerm = "",
               [FromQuery] string sortColumn = "ColorName",
               [FromQuery] string sortDirection = "asc",
               [FromQuery] int page = 1,
               [FromQuery] int pageSize = 10)
        {
            
            IQueryable<Material> query = _context.Materials.AsQueryable().Where(b => b.IsActive == true);

            if (!string.IsNullOrEmpty(searchTerm))
            {

                query = query.Where(b => b.ColorName.Contains(searchTerm));


                var tempResults = await query.ToListAsync();

                // Apply additional client-side filters only if needed
                if (tempResults.Count < pageSize ||
                    tempResults.All(b => b.ColorName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                {
                    // Get ALL active bills if preliminary filter was too restrictive
                    tempResults = await _context.Materials
                        .Where(b => b.IsActive == true)
                        .ToListAsync();
                }

                // Apply comprehensive filtering
                var filteredResults = tempResults
                    .Where(b =>
                        b.ColorName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.BasePrice.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();


                query = filteredResults.AsQueryable();
            }

                    switch (sortColumn)
                    {
                        case "BuyerName":
                            query = sortDirection == "asc"
                                ? query.OrderBy(b => b.ColorName)
                                : query.OrderByDescending(b => b.ColorName);
                            break;
                        case "BasePrice":
                            query = sortDirection == "asc"
                                ? query.OrderBy(b => b.BasePrice)
                                : query.OrderByDescending(b => b.BasePrice);
                            break;
                        default: // "baseprice"
                            query = sortDirection == "asc"
                                ? query.OrderBy(b => b.ColorName)
                                : query.OrderByDescending(b => b.ColorName);
                            break;
                    }

                    // Pagination
                    int totalItems = query.Count();
                    int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                    var Materials = query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                   var materials = new
                    {
                        Materials = Materials,
                        SearchTerm = searchTerm,
                        SortColumn = sortColumn,
                        SortDirection = sortDirection,
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalItems = totalItems,
                        TotalPages = totalPages
                    };

                    return Ok(materials);
        }

        // GET: api/Materials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Material>> GetMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);

            if (material == null)
            {
                return NotFound();
            }

            return material;
        }

        // POST: api/Materials
        [HttpPost("CreateMaterial")]
        public async Task<ActionResult<Material>> CreateMaterial([FromBody] Material material)
        {
            material.IsActive = true;
            var Exit = _context.Materials.FirstOrDefault(m=>m.ColorName == material.ColorName);

            if (Exit!=null)
            {
                Exit.BasePrice = material.BasePrice;
                Exit.IsActive = true;
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetMaterial", new { id = material.Id }, material);
            }
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMaterial", new { id = material.Id }, material);
        }

        [HttpPut]
        [Route("Modify/{id}")]
        public async Task<IActionResult> Modify(int id, [FromBody] Material material)
        {
            if (id != material.Id)
            {
                return BadRequest("ID in the URL does not match the ID in the body.");
            }

            _context.Entry(material).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialExists(id))
                {
                    return NotFound($"Material with ID {id} not found.");
                }
                else
                {
                    // Log the exception details
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Materials/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
         
            if (material == null)
            {
                return NotFound();
            }
           
            material.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
}
