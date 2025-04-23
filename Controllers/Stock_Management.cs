using MAPI.Models;
using MAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace MAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Stock_Management : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly StockService _stockService;

        public Stock_Management(AppDbContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }


        [HttpGet("GetAllStocks")]
        public async Task<IActionResult> GetAllStocks(
    [FromQuery] string searchTerm = "",
    [FromQuery] string sortColumn = "ColorName",
    [FromQuery] string sortDirection = "asc",
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12)
        {
            IQueryable<Stock> query = _context.Stocks
                .Include(s => s.Material)
                .Where(s => s.isActive);

            // Server-side filtering
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s =>
                    s.Material.ColorName.Contains(searchTerm) ||
                    s.TotalBags.ToString().Contains(searchTerm) ||
                    s.AvailableStock.ToString().Contains(searchTerm) ||
                    (s.TotalBags * s.Weight).ToString().Contains(searchTerm)
                );
            }

            // Sorting
            switch (sortColumn)
            {
                case "ColorName":
                    query = sortDirection == "asc" ? query.OrderBy(s => s.Material.ColorName) : query.OrderByDescending(s => s.Material.ColorName);
                    break;
                case "TotalBags":
                    query = sortDirection == "asc" ? query.OrderBy(s => s.TotalBags) : query.OrderByDescending(s => s.TotalBags);
                    break;
                case "AvailableStock":
                    query = sortDirection == "asc" ? query.OrderBy(s => s.AvailableStock) : query.OrderByDescending(s => s.AvailableStock);
                    break;
                default:
                    query = sortDirection == "asc" ? query.OrderBy(s => s.StockId) : query.OrderByDescending(s => s.StockId);
                    break;
            }

            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedStocks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new Stock_
                {
                    StockId = s.StockId,
                    MaterialId = s.MaterialId,
                    ColorName = s.Material.ColorName,
                    TotalBags = s.TotalBags,
                    Weight = s.Weight,
                    AvailableStock = s.AvailableStock,
                    isActive = s.isActive
                })
                .ToListAsync();

            var viewModel = new stockListViewModel
            {
                Stock = pagedStocks,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            return Ok(viewModel);
        }


        [HttpGet("{stockId}")]

        public async Task<IActionResult> GetStockById(int stockId)
        {
            try
            {
                var stock = await _stockService.GetStockByIdAsync(stockId);
                return Ok(stock);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Stock not found.");
            }
        }
        // Create stock entry

        [HttpPost("CreateStock")]
        public async Task<IActionResult> CreateStock([FromBody] Stock createStockDto)
        {
            if (createStockDto == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var stock = await _stockService.CreateStockAsync(createStockDto.MaterialId, createStockDto.TotalBags, createStockDto.Weight);
                return CreatedAtAction(nameof(GetStockById), new { stockId = stock.StockId }, stock);
            }
            catch (ArgumentException e)
            {

                return BadRequest(e.Message);
            }
        }
        // Update stock details
        [HttpPut("{stockId}")]
        public async Task<IActionResult> UpdateStock(int stockId, [FromBody] Stock updateStockDto)
        {
            try
            {
                var updatedStock = await _stockService.UpdateStockAsync(stockId, updateStockDto.TotalBags, updateStockDto.Weight);
                return Ok(updatedStock);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Stock not found.");
            }
        }


        [HttpPut("{stockId}/add_stock")]
        public async Task<IActionResult> Add_Stocks(int stockId, [FromBody] Stock addedStockDto)
        {
            if (addedStockDto == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                // Call the service to update the stock
                var addedStock = await _stockService.Add_Stocks_per_item(stockId, addedStockDto);

                // Return the updated stock
                return Ok(addedStock);
            }
            catch (KeyNotFoundException)
            {
                // Stock not found
                return NotFound("Stock not found.");
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{stockId}/remove_stock")]
        public async Task<IActionResult> remove_stock(int stockId, [FromBody] Stock removeStockDto)
        {
            if (removeStockDto == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                // Call the service to remove the stock item
                var removedStock = await _stockService.remove_Stocks_per_item(stockId, removeStockDto);

                // Return the updated stock
                return Ok(removedStock);
            }
            catch (KeyNotFoundException)
            {
                // Stock not found
                return NotFound("Stock not found.");
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Delete stock entry
        [HttpDelete("{stockId}")]
        public async Task<IActionResult> DeleteStock(int stockId)
        {
            try
            {
                await _stockService.DeleteStockAsync(stockId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Stock not found.");
            }
        }


    }


}


