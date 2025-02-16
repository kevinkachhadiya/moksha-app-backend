using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAPI.Models;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<ActionResult<IEnumerable<Material>>> GetMaterials()
        {
            try
            {
                var materials = await _context.Materials.ToListAsync();
                return Ok(materials);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMaterials: {ex}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
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

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
}
