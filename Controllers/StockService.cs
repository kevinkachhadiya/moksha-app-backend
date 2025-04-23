using Microsoft.EntityFrameworkCore;
using MAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MAPI.Controllers;

namespace MAPI.Services
{
    public class StockService : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BillingServices _b;

        public StockService(AppDbContext context, BillingServices b)
        {
            _context = context;
            _b = b;
        }

        // Create a new stock entry
        public async Task<Stock> CreateStockAsync(int materialId, int totalBags, decimal weightPerBag)
        {
            var materialExists = await _context.Materials.AnyAsync(m => m.Id == materialId);
            var AlreadyDeletedStock = await _context.Stocks.Where(s=>s.isActive==false).FirstOrDefaultAsync(s=>s.MaterialId == materialId);
            var already_available  = await _context.Stocks.Where(s => s.isActive == true).FirstOrDefaultAsync(s => s.MaterialId == materialId);
            if (!materialExists)
            {
                throw new ArgumentException("Material does not exist.");
            }
            if (weightPerBag <= 0 || totalBags <= 0)
            {
                throw new ArgumentException("Invalid values for weight or bags.");
            }
            if (AlreadyDeletedStock != null)
            {
                AlreadyDeletedStock.isActive = true;
                AlreadyDeletedStock.TotalBags = totalBags;
                AlreadyDeletedStock.Weight = weightPerBag;
                AlreadyDeletedStock.AvailableStock = AlreadyDeletedStock.TotalWeight;
                await _context.SaveChangesAsync();
                return AlreadyDeletedStock;
            }
            else if (already_available!=null)
            {
                throw new ArgumentException("can not create duplicated material");
            }
            else {
                var stock = new Stock
                {
                    MaterialId = materialId,
                    TotalBags = totalBags,
                    Weight = weightPerBag,
                    isActive = true,
                    AvailableStock = totalBags*Weight
                };

                // Add to DB context and save
                _context.Stocks.Add(stock);
                await _context.SaveChangesAsync();

                return stock;
            }
        }

        // Retrieve stock by its ID
        public async Task<Stock> GetStockByIdAsync(int stockId)
        {
            var stock = await _context.Stocks
                .Include(s => s.Material) // Include Material details if needed
                .FirstOrDefaultAsync(s => s.StockId == stockId);

            if (stock == null)
            {
                throw new KeyNotFoundException("Stock not found.");
            }

            return stock;
        }

        // Update the stock's details
        public async Task<Stock> UpdateStockAsync(int stockId, int bagsAdded, decimal weightPerBag)
        {
            var stock = await _context.Stocks.FindAsync(stockId);

            if (stock == null)
            {
                throw new KeyNotFoundException("Stock not found.");
            }

            // Validate the inputs
            if (bagsAdded <= 0 || weightPerBag <= 0)
            {
                throw new ArgumentException("Invalid values for bags or weight.");
            }

            // Update the stock details
            stock.TotalBags = bagsAdded;
            stock.Weight = weightPerBag;

            stock.AvailableStock =
            // Save changes
            await _context.SaveChangesAsync();

            return stock;
        }

        // Delete a stock entry
        public async Task DeleteStockAsync(int stockId)
        {
            var stock = await _context.Stocks.FindAsync(stockId);

            if (stock == null)
            {
                throw new KeyNotFoundException("Stock not found.");
            }

            // Remove the stock record from the database
            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
        }
        // Get all stocks
        public async Task<List<Stock>> GetAllStocksAsync()
        {
            return await _context.Stocks
                .Include(s => s.Material)  // Optional: Include Material information if needed
                .ToListAsync();
        }
        public async Task<Stock> Add_Stocks_per_item(int stockId, Stock updateStockDto)
        {
            // Find the stock by ID
            var stock = await _context.Stocks.FindAsync(stockId);
            if (stock == null)
            {
                throw new KeyNotFoundException("Stock not found.");
            }

            // Update the stock with new values
            stock.TotalBags = updateStockDto.TotalBags;
            stock.Weight = updateStockDto.Weight;

            // Update the AvailableStock based on the new values
            stock.AddStock(stock.TotalBags, stock.Weight);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return stock;
        }
        public async Task<Stock> remove_Stocks_per_item(int stockId, Stock updateStockDto)
        {
            // Find the stock by ID
            var stock = await _context.Stocks.FindAsync(stockId);
            if (stock == null)
            {
                throw new KeyNotFoundException("Stock not found.");
            }

            // Update the stock with new values
            stock.TotalBags = updateStockDto.TotalBags;
            stock.Weight = updateStockDto.Weight;

            // Update the AvailableStock based on the new values
            stock.RemoveStock(stock.TotalWeight);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return stock;
        }


        /*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        public async Task<S_Bill> S_CreateBillAsync(S_Bill newBill)
        {
            if (newBill == null)
            {
                throw new ArgumentNullException(nameof(newBill));
            }

            // Ensure the bill has items
            if (newBill.Items == null || !newBill.Items.Any())
            {
                throw new InvalidOperationException("A bill must have at least one item.");
            }
            // Set CreatedAt to current time if not set
            newBill.CreatedAt = DateTime.UtcNow;
            newBill.DueDate = newBill.DueDate;
            newBill.Status = newBill.Status;
            newBill.RemainAmount = newBill.RemainAmount;
            newBill.S_BillNo = await Generate_s_BillNoAsync();

            // Create a new transaction scope to ensure that the bill and stock changes are atomic
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    decimal Total = 0;

                    // Loop through each item in the bill and validate stock
                    foreach (var item in newBill.Items)
                    {
                        var I_stock = await _context.Stocks.Include(m => m.Material).FirstOrDefaultAsync(s => s.StockId == item.P_Stock_Id);
                        item.Stock = I_stock;
                        if (I_stock == null)
                        {
                            throw new InvalidOperationException($"Stock with ID {item.P_Stock_Id} does not exist.");

                        }
                        // Ensure there's enough stock available
                        if (I_stock.AvailableStock < item.S_totalWeight)
                        {
                            throw new InvalidOperationException($"Not enough stock available for item {item.P_Stock_Id}.");
                        }
                        // Deduct the stock amount
                        if (item.S_totalWeight <= I_stock.AvailableStock)
                        {
                            I_stock.AvailableStock -= item.S_totalWeight;
                        }

                        // Explicitly mark the stock as modified
                        _context.Stocks.Update(I_stock);

                        // Update the item price based on total weight
                        item.price = item.price;
                        Total += item.price * item.S_totalWeight ;

                       
                        _context.S_BillItems.Add(item);
                        await _context.SaveChangesAsync();
                    }

                    // Set the total price for the bill
                    newBill.Total_S_Price = Total;

                    // Add the new bill to the context
                    _context.S_Bills.Add(newBill);

                    // Save changes to the database (bill and stock updates)
                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();

                    return newBill;
                }
                catch (Exception)
                {
                    // Rollback transaction in case of error
                    await transaction.RollbackAsync();
                    throw;  // Re-throw the exception after rolling back
                }
            }

        }

        // Modify an existing Bill (Update Bill)
        public async Task<ActionResult<S_Bill>> S_ModifyBillAsync(int billId, S_Bill updatedBill)
        {
            // Check if the provided bill data is valid
            if (updatedBill == null || updatedBill.S_Id != billId)
            {
                return BadRequest("Bill data is invalid.");
            }

            // Fetch the bill from the database along with the associated items and their stocks
            var existingBill = await _context.S_Bills.Include(b => b.Items)
                                                      .ThenInclude(i => i.Stock)  // Fetch related stock
                                                      .ThenInclude(m => m.Material)
                                                      .FirstOrDefaultAsync(b => b.S_Id == billId);

            if (existingBill == null)
            {
                return NotFound();  // Return 404 if the bill doesn't exist
            }

            existingBill.Total_S_Price = 0; // Reset the total price of the bill

            // Update bill properties
            existingBill.S_BillNo = updatedBill.S_BillNo;
            existingBill.SellerName = updatedBill.SellerName;
            existingBill.PaymentMethod = updatedBill.PaymentMethod;
            existingBill.CreatedAt = updatedBill.CreatedAt;
            existingBill.IsPaid = updatedBill.IsPaid;
            existingBill.DueDate = updatedBill.DueDate;
            existingBill.Status = updatedBill.Status;
            existingBill.RemainAmount = updatedBill.RemainAmount;


            // Track updated item IDs
            var updatedItemIds = new HashSet<int>();

            // Loop through each item from the updated bill
            foreach (var item in updatedBill.Items)
            {
                var stock = await _context.Stocks.Include(i => i.Material)
                                                 .FirstOrDefaultAsync(i => i.StockId == item.P_Stock_Id);

                if (stock != null)
                {
                    // Get the existing item if it already exists in the bill
                    var existingItem = existingBill.Items.FirstOrDefault(i => i.P_Stock_Id == item.P_Stock_Id);
                    decimal previous_stock = existingItem?.S_totalWeight ?? 0;

                    if (existingItem != null)
                    {
                        // Update existing item properties
                        existingItem.St_Bags = item.St_Bags;
                        existingItem.St_Weight = item.St_Weight;
                        existingItem.price = item.price;

                        // Update the bill's total price using the total weight for the item
                        existingBill.Total_S_Price += existingItem.price * existingItem.S_totalWeight;

                        // Calculate weight difference
                        decimal stock_diff = item.S_totalWeight - previous_stock;
                        var available_stk = stock.AvailableStock;

                        if ( (available_stk-stock_diff)>0)
                        {
                            // Adjust stock based on the difference in weight
                            stock.AvailableStock -= stock_diff;
                            _context.Stocks.Update(stock);
                        }
                        else
                        {
                            throw new InvalidOperationException($"available stock of material is lesser than your requirement");
                        }

                        // Mark this item as updated
                        updatedItemIds.Add(existingItem.ID);
                    }
                    else
                    {
                        // Create new S_BillItem if the item is not already in the bill
                        var newItem = new S_BillItem
                        {
                            P_Stock_Id = item.P_Stock_Id,
                            St_Bags = item.St_Bags,
                            St_Weight = item.St_Weight,
                            price = item.price,
                            Stock = stock,
                        };

                        existingBill.Items.Add(newItem);
                        existingBill.Total_S_Price += newItem.price * newItem.S_totalWeight;

                        // Adjust stock for the newly added item
                        decimal stock_diff = item.S_totalWeight;

                        if (stock_diff > 0)
                        {
                            if (stock.AvailableStock < stock_diff)
                            {
                                return BadRequest($"Not enough stock available for item {item.P_Stock_Id}.");
                            }

                            stock.AvailableStock -= stock_diff; // Subtract from available stock
                            _context.Stocks.Update(stock);
                        }

                        updatedItemIds.Add(newItem.ID);
                    }
                }
                else
                {
                    // Return bad request if stock for the item doesn't exist
                    return BadRequest($"Stock with ID {item.P_Stock_Id} not found.");
                }
            }

            // Handle removal of items no longer in the updated bill
            var itemsToRemove = existingBill.Items
                   .Where(i => !updatedItemIds.Contains(i.ID))
                   .ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                existingBill.Items.Remove(itemToRemove);

                // Update stock to revert the item weight
                var stockToRemove = await _context.Stocks.Include(m => m.Material)
                                                         .FirstOrDefaultAsync(s => s.StockId == itemToRemove.P_Stock_Id);

                if (stockToRemove != null)
                {
                    stockToRemove.AvailableStock += itemToRemove.S_totalWeight; // Add back the weight of the removed item
                    _context.Stocks.Update(stockToRemove);
                }
            }
            
            // Save changes to the database (both the bill and stock details)
            try
            {
                await _context.SaveChangesAsync();
                return NoContent();  // Return the updated bill
            }
            catch (Exception ex)
            {
                // Log error and return an internal server error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Delete a Bill (and cascade delete its items if necessary)
        public async Task S_DeleteBillAsync(int billId)
        {
            // Fetch the bill to delete along with its items
            var billToDelete = await _context.S_Bills.Include(b => b.Items).FirstOrDefaultAsync(b => b.S_Id == billId);
            if (billToDelete == null)
                throw new InvalidOperationException("Bill not found.");

            // Remove all bill items related to the bill
            _context.S_BillItems.RemoveRange(billToDelete.Items);

            // Remove the bill itself
            _context.S_Bills.Remove(billToDelete);

            // Save changes to DB
            await _context.SaveChangesAsync();
        }

        // Get a list of all bills
        public async Task<List<S_Bill>> GetAllBillsAsync()
        {
            return await _context.S_Bills.Include(b => b.Items).ThenInclude(s=>s.Stock).ThenInclude(s => s.Material).ToListAsync();
        }

        // Get a specific bill by its ID
        public async Task<S_Bill> GetBillByIdAsync(int billId)
        {
            return await _context.S_Bills.Include(b => b.Items).ThenInclude(s => s.Stock).ThenInclude(s => s.Material).FirstOrDefaultAsync(b => b.S_Id == billId);
        }

        // Optional: A helper method to update stock when items are removed from the bill
        public async Task S_RemoveStockFromBillItemsAsync(S_Bill bill)
        {
            foreach (var item in bill.Items)
            {
                // Check if stock is available, and remove the stock
                if (item.Stock != null)
                {
                    var stockToUpdate = await _context.Stocks.FindAsync(item.P_Stock_Id);
                    if (stockToUpdate != null)
                    {
                        var totalWeightToRemove = item.S_totalWeight;
                        if (stockToUpdate.AvailableStock >= totalWeightToRemove)
                        {
                            stockToUpdate.AvailableStock -= totalWeightToRemove;
                        }
                        else
                        {
                            throw new InvalidOperationException("Not enough stock available.");
                        }

                        _context.Stocks.Update(stockToUpdate);
                    }
                }
            }

            // Save changes to the stock data
            await _context.SaveChangesAsync();
        }

        private async Task<string> Generate_s_BillNoAsync()
        {
            // Get the most recent bill to generate the next BillNo
            var lastBill = await _context.S_Bills
                                          .OrderByDescending(b => b.CreatedAt)
                                          .FirstOrDefaultAsync();

            // If no bills exist, start with B_0
            if (lastBill == null)
            {
                return "S_1";
            }

            // Extract the last number from the BillNo and increment it
            var lastNumber = lastBill.S_BillNo.Substring(2);  // Get the number part after "B_"
            if (int.TryParse(lastNumber, out int number))
            {
                return $"S_{number + 1}"; // Increment the number
            }

            return "S_1"; // Fallback if the format doesn't match
        }

        public async Task<IActionResult> WithOut_Gst(int billId)
        {
            // Fetch the bill with related items
            var bill = await _context.S_Bills
                                     .Include(b => b.Items)       // Include related Items
                                     .ThenInclude(s=>s.Stock)
                                     .ThenInclude(i => i.Material) // Eagerly load Material if it's a navigation property
                                     .FirstOrDefaultAsync(b => b.S_Id == billId);

            if (bill == null)
            {
                return NotFound();  // Return 404 if bill is not found
            }

            // Generate the invoice PDF
            var fileName = $"{bill.S_BillNo}_Invoice.pdf";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // _b.GenerateInvoice is a method that generates the PDF
             _b.Generate_Invoice_WithOut_Gst(filePath, new List<S_Bill> { bill });

            // Read the PDF file into a byte array
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            // Return the file to the client with proper content type
            return File(fileBytes, "application/pdf", fileName);
        }


    }
}


