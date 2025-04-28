using MAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using static iTextSharp.text.pdf.AcroFields;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<B_Bill>>> GetBills(
               [FromQuery] string searchTerm="",
               [FromQuery] string sortColumn = "CreatedAt",
               [FromQuery] string sortDirection = "desc",
               [FromQuery] int page = 1,
               [FromQuery] int pageSize = 10)
        {


            IQueryable<B_Bill> query = _context.B_Bill.AsQueryable().Where(b=>b.IsActive==true);


            if (!string.IsNullOrEmpty(searchTerm))
            {
                // First try database-compatible filters
                query = query.Where(b => b.BuyerName.Contains(searchTerm));

                // Get preliminary results
                var tempResults = await query.ToListAsync();

                // Apply additional client-side filters only if needed
                if (tempResults.Count < pageSize ||
                    tempResults.All(b => b.BuyerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                {
                    // Get ALL active bills if preliminary filter was too restrictive
                    tempResults = await _context.B_Bill
                        .Where(b => b.IsActive == true)
                        .ToListAsync();
                }

                // Apply comprehensive filtering
                var filteredResults = tempResults
                    .Where(b =>
                        b.BuyerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.PaymentMethod.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.TotalBillPrice.ToString(CultureInfo.InvariantCulture).Contains(searchTerm) ||
                        b.CreatedAt.ToString("yyyy-MM-dd").Contains(searchTerm))
                    .ToList();
                query = filteredResults.AsQueryable();
            }

            switch (sortColumn)
            {
                case "BuyerName":
                    query = sortDirection == "asc"
                        ? query.OrderBy(b => b.BuyerName)
                        : query.OrderByDescending(b => b.BuyerName);
                    break;
                case "TotalBillPrice":
                    query = sortDirection == "asc"
                        ? query.OrderBy(b => b.TotalBillPrice)
                        : query.OrderByDescending(b => b.TotalBillPrice);
                    break;
                case "PaymentMethod":
                    query = sortDirection == "asc"
                        ? query.OrderBy(b => b.PaymentMethod)
                        : query.OrderByDescending(b => b.PaymentMethod);
                    break;
                default: // "CreatedAt"
                    query = sortDirection == "asc"
                        ? query.OrderBy(b => b.CreatedAt)
                        : query.OrderByDescending(b => b.CreatedAt);
                    break;
            }

            // Pagination
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var bills = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var Bills = new
            {
                Bills = bills,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
        };

            return Ok(Bills);
        }

        [HttpGet("GetBillByid")]
        public async Task<IActionResult> GetBill(int billId)
        {
            var bill = await _context.B_Bill
                                     .Include(b => b.Items)
                                     .ThenInclude(i => i.Material)
                                     .FirstOrDefaultAsync(b => b.B_Id == billId && b.IsActive == true);

            dynamic editBillDto;

            if (bill != null)
            {
                  editBillDto = new Create_B_Bill_Dto
                {
                    BuyerName = bill.BuyerName,
                    P_number =  bill.P_number,
                    IsPaid = bill.IsPaid,
                    PaymentMethod = bill.PaymentMethod,
                    Items = new List<B_BillItemDto>(),
                   

                };
            }
            else
            {
                return NotFound();
            }

            foreach (var item in bill.Items)
            {
               
                editBillDto.Items.Add(new B_BillItemDto
                {
                    MaterialId = item.MaterialId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });

            }

            return Ok(editBillDto);
        }
              
        [HttpPost]
        public async Task<IActionResult> CreateBill([FromBody] Create_B_Bill_Dto Dto_b_bill)
        {
            var supplier = await _context.Party.FirstOrDefaultAsync(p=>p.P_number == Dto_b_bill.P_number && p.P_Name == Dto_b_bill.BuyerName);

            if (supplier == null)
            {
                return BadRequest("Supplier not found, check name.");

            }

            var bill = new B_Bill()
            {
                BillNo = await GenerateBillNoAsync(),
                BuyerName = Dto_b_bill.BuyerName,
                P_number  = Dto_b_bill.P_number,
                Items = new List<B_BillItem>(),
                IsPaid = Dto_b_bill.IsPaid,
                PaymentMethod = Dto_b_bill.PaymentMethod,
                IsActive = true
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

        [HttpPut("UpdateBill")]
        public async Task<IActionResult> UpdateBill([FromBody] Edit_B_Bill_Dto Dto_b_bill)
        {
            try
            {
              var old_bill = _context.B_Bill.Include(b=>b.Items).FirstOrDefault(b=>b.B_Id == Dto_b_bill.id);

                var supplier = _context.Party.FirstOrDefault(p=>p.P_number == Dto_b_bill.P_number && p.P_Name == p.P_Name);

                if (old_bill != null)
                {

                    if (supplier != null)
                    {
                        old_bill.BuyerName = Dto_b_bill.BuyerName;
                        old_bill.P_number = Dto_b_bill.P_number;
                        old_bill.Items.Clear();
                        old_bill.Items = new List<B_BillItem>();
                        old_bill.IsPaid = Dto_b_bill.IsPaid;
                        old_bill.CreatedAt = DateTime.UtcNow;
                        old_bill.PaymentMethod = Dto_b_bill.PaymentMethod;
                    }
                    else
                    {

                        return BadRequest("Name and Number is not same as supplier section");
                     }

                }
            else
            {
                return BadRequest("Bill data is null.");
            }
            old_bill.TotalBillPrice = 0;
            foreach (var item in Dto_b_bill.Items)
            {
                var material = await _context.Materials.FindAsync(item.MaterialId);
                var w_item = new B_BillItem() { };
                if (material != null)
                {
                    w_item.MaterialId = material.Id;
                    w_item.Quantity = item.Quantity;
                    w_item.Price = item.Price;
                    w_item.Material = material;

                  
                    old_bill.TotalBillPrice += w_item.TotalPrice;  // item.TotalPrice is Price * Quantity

                     old_bill.Items.Add(w_item);
                }
                else
                {
                    return BadRequest($"Material with ID {item.MaterialId} not found.");
                }
            }

            if (old_bill.IsPaid)
            {
                await _context.SaveChangesAsync();
                return Accepted();
            }
            else
            {
                return BadRequest("CheckBox is not marked");
            }

            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The bill was modified by another user.");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }


        }

        [HttpGet("print_buying_bile")]
        public async Task<IActionResult> GenerateInvoiceForBill(int billId)
        {
            var bill = await _context.B_Bill
                                     .Include(b => b.Items)
                                     .ThenInclude(i => i.Material) // Eagerly load Material
                                     .FirstOrDefaultAsync(b => b.B_Id == billId && b.IsActive == true);

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
            try
            {
                return File(fileBytes, "application/pdf", "Invoice.pdf"); // Return for download
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        [HttpDelete("Deletebill")]
        public async Task<IActionResult> DeleteBill(int billId)
        {
            try
            {

                var bill = await _context.B_Bill
                                     .Include(b => b.Items)
                                     .FirstOrDefaultAsync(b => b.B_Id == billId);
                if (bill != null)
                {

                    bill.IsActive = false;
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                else
                {
                    return BadRequest($"can't find bill");
                
                }
               

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // Return a 500 status if there's an error
            }

          
        }
    
    }
}

    


