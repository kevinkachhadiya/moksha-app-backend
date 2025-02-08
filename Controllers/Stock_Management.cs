using MAPI.Models;
using MAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet]
        public async Task<IActionResult> GetAllStocks()
        {
            try
            {
                var stocks = await _stockService.GetAllStocksAsync();
                if (stocks == null || stocks.Count == 0)
                {
                    return NotFound("No stocks found.");
                }

                return Ok(stocks);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // Retrieve stock by ID
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

        [HttpPost]
        public async Task<IActionResult> CreateStock([FromBody] Stock createStockDto)
        {
            if (createStockDto == null)
            {
                return BadRequest("Invalid data.");
            }

            var stock = await _stockService.CreateStockAsync(createStockDto.MaterialId, createStockDto.TotalBags, createStockDto.Weight);
            return CreatedAtAction(nameof(GetStockById), new { stockId = stock.StockId }, stock);
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


