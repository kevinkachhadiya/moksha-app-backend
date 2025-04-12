using MAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace MAPI.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerBillingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BillingServices _b;

        public BuyerBillingController(AppDbContext context, BillingServices b)
        {
            _context = context;
            _b = b;
        }

        // GET: api/BuyerBilling
        [HttpGet]
        public async Task<ActionResult<IEnumerable<B_Bill>>> GetBills()
        {
            var bill = await _context.B_Bill
                             .Include(b => b.Items)          // Include the items
                             .ThenInclude(i => i.Material)   // Include the related material for each item
                             .ToListAsync();
            return bill;
        }

        // GET: api/BuyerBilling/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBill(int billId)
        {
            var bill = await _context.B_Bill
                                     .Include(b => b.Items)
                                     .ThenInclude(i => i.Material) // Eagerly load Material
                                     .FirstOrDefaultAsync(b => b.B_Id == billId);

            if (bill == null)
            {
                return NotFound();
            }

            return Ok(bill);
        }
        // POST: api/BuyerBilling
        
        [HttpPost]
        public async Task<IActionResult> CreateBill([FromBody] Create_B_Bill_Dto Dto_b_bill)
        {
            var bill = new B_Bill()
            {
                BillNo = await GenerateBillNoAsync(),
                BuyerName = Dto_b_bill.BuyerName,
                // Don't set B_Id - let the database generate it
                Items = new List<B_BillItem>(),
                IsPaid = Dto_b_bill.IsPaid,
                PaymentMethod = Dto_b_bill.PaymentMethod
            };


            if (Dto_b_bill == null)
            {
                return BadRequest("Bill data is null.");
            }

            // Generate the BillNo before saving
            bill.BillNo = await GenerateBillNoAsync();
            bill.TotalBillPrice = 0;
            
            foreach (var item in Dto_b_bill.Items)
            {
                // Fetch the material from the database using MaterialId
                var material = await _context.Materials.FindAsync(item.MaterialId);
                var w_item = new B_BillItem() {};
                


                if (material != null)
                {
                    w_item.MaterialId = material.Id;
                    w_item.Quantity = item.Quantity;
                    w_item.Price = item.Price;
                    w_item.Material = material;

                    // Add the item's total price to the total bill price
                    bill.TotalBillPrice += w_item.TotalPrice;  // item.TotalPrice is Price * Quantity

                    bill.Items.Add(w_item);
                }
                else
                {
                    // If material is not found, return a bad request response
                    return BadRequest($"Material with ID {item.MaterialId} not found.");
                }
            }

            if (bill.IsPaid == true)
            {
                _context.B_Bill.Add(bill);

            }
            else
            {
                return BadRequest("CheckBox is not marked");
            }
            try
            {
                // Save the changes to the database
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error and return an internal server error
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            // Return the newly created bill as a response
            return CreatedAtAction(nameof(GetBill), new { id = bill.B_Id }, bill);
        }

        // Helper method to generate BillNo
        private async Task<string> GenerateBillNoAsync()
        {
            // Get the most recent bill to generate the next BillNo
            var lastBill = await _context.B_Bill
                                          .OrderByDescending(b => b.CreatedAt)
                                          .FirstOrDefaultAsync();

            // If no bills exist, start with B_0
            if (lastBill == null)
            {
                return "B_1";
            }

            // Extract the last number from the BillNo and increment it
            var lastNumber = lastBill.BillNo.Substring(2);  // Get the number part after "B_"
            if (int.TryParse(lastNumber, out int number))
            {
                return $"B_{number + 1}"; // Increment the number
            }

            return "B_1"; // Fallback if the format doesn't match
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBill(int id, [FromBody] B_Bill bill)
        {
            if (bill == null || bill.B_Id != id)
            {
                return BadRequest("Bill data is invalid.");
            }

            // Fetch the bill from the database along with the associated items and materials
            var existingBill = await _context.B_Bill.Include(b => b.Items)
                                                    .ThenInclude(i => i.Material)
                                                    .FirstOrDefaultAsync(b => b.B_Id == id);

            if (existingBill == null)
            {
                return NotFound(); // Return 404 if the bill doesn't exist
            }

            // Update the properties of the existing bill
            existingBill.BuyerName = bill.BuyerName;
            existingBill.PaymentMethod = bill.PaymentMethod;
            existingBill.CreatedAt = bill.CreatedAt;
            existingBill.IsPaid = bill.IsPaid;

            // Initialize the total price for the existing bill to 0
            existingBill.TotalBillPrice = 0;

            // Create a set to track the updated item IDs
            var updatedItemIds = new HashSet<int>();

            // Loop through each item from the incoming bill
            foreach (var item in bill.Items)
            {
                // Fetch the material from the database
                var material = await _context.Materials.FindAsync(item.MaterialId);

                if (material != null)
                {
                    // Check if the item already exists in the current bill
                    var existingItem = existingBill.Items.FirstOrDefault(i => i.MaterialId == item.MaterialId);

                    if (existingItem != null)
                    {
                        // Update existing item properties
                        existingItem.Quantity = item.Quantity;
                        existingItem.Price = item.Price;
                        existingItem.Material = material;  

                        // Update the bill's total price
                        existingBill.TotalBillPrice += existingItem.TotalPrice;

                        // Mark this item as updated
                        updatedItemIds.Add(existingItem.Id);
                    }
                    else
                    {
                        // Create a new item and add it to the bill's Items collection
                        var newItem = new B_BillItem
                        {
                            MaterialId = item.MaterialId,
                            Quantity = item.Quantity,
                            Price = item.Price, 
                        };
                        newItem.Material = material;

                        // Add the new item to the bill's Items collection
                        existingBill.Items.Add(newItem);  // Important step here
                        updatedItemIds.Add(newItem.Id);

                        // Add the new item’s total price to the bill’s total price
                        existingBill.TotalBillPrice += newItem.TotalPrice;
                    }
                }
                else
                {
                    // Return a bad request if the material is not found
                    return BadRequest($"Material with ID {item.MaterialId} not found.");
                }
            }

            // Remove items that are no longer part of the updated bill
            var itemsToRemove = existingBill.Items.Where(i => !updatedItemIds.Contains(i.Id)).ToList();
            foreach (var itemToRemove in itemsToRemove)
            {
                existingBill.Items.Remove(itemToRemove);
            }

            try
            {
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return an internal server error
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            // Return a NoContent response indicating the update was successful
            return NoContent();
        }

        [HttpGet("print_buying_bile")]
        public async Task<IActionResult> GenerateInvoiceForBill(int billId)
        {
            var bill = await _context.B_Bill
                                     .Include(b => b.Items)
                                     .ThenInclude(i => i.Material) // Eagerly load Material
                                     .FirstOrDefaultAsync(b => b.B_Id == billId);

            if (bill == null)
            {
                return NotFound();
            }

            // Generate the invoice PDF
            var fileName = $"{bill.BillNo}_Invoice.pdf"; // Example file name: B_123_Invoice.pdf
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName); // Save to wwwroot folder

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Ensure directory exists
            }
            _b.GenerateInvoice(filePath, new List<B_Bill> { bill });

            // Return the PDF as a file download
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath); // Read the file into a byte array
            return File(fileBytes, "application/pdf", fileName); // Return the file with a content type of PDF
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBill(int id)
        {
            // Find the bill including related items
            var bill = await _context.B_Bill
                                     .Include(b => b.Items)
                                     .FirstOrDefaultAsync(b => b.B_Id == id);

            if (bill == null)
            {
                return NotFound(); // Return 404 if the bill doesn't exist
            }

            // Remove all related items from B_BillItem table
            _context.B_BillItem.RemoveRange(bill.Items);

            // Remove the bill itself
            _context.B_Bill.Remove(bill);

            try
            {
                // Save the changes to the database
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // Return a 500 status if there's an error
            }

            return NoContent(); // Return 204 No Content after successful deletion
        }
    }
}

    


