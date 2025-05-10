using iText.Commons.Actions.Contexts;
using MAPI.Models;
using MAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerBillingController : ControllerBase
    {

        private readonly StockService _Service_context;
        private readonly AppDbContext _b1;
        private readonly BillingServices _b2;

        public SellerBillingController(StockService context, BillingServices b2, AppDbContext b1)
        {

            _Service_context = context;
            _b1 = b1;
            _b2 = b2;

        }
        /*
      //  GET: api/Bill/{id}
       [HttpGet("GetBill/{id}")]
       public async Task<IActionResult> GetBill(int id)
       {
           var bill = 1;

           if (bill == null)
               return NotFound("Bill not found.");

           return Ok(bill);
       }

      // GET: api/Bill
      [HttpGet("GetBills")]
      public async Task<IActionResult> GetBills()
      {
          var bills = await _Service_context.GetAllBillsAsync();
          return Ok(bills);
      }

      // POST: api/Bill
      [HttpPost]
      public async Task<IActionResult> CreateBill([FromBody] S_Bill newBill)
      {
          if (newBill == null)
              return BadRequest("Bill data is invalid.");

          var createdBill = await _Service_context.S_CreateBillAsync(newBill);
          return CreatedAtAction(nameof(GetBill), new { id = createdBill.S_Id }, createdBill);
      }

      // PUT: api/Bill/{id}
      [HttpPut("ModifyBill/{id}/Modifing")]
      public async Task<IActionResult> ModifyBill(int id, [FromBody] S_Bill updatedBill)
      {
          if (updatedBill == null)
              return BadRequest("Updated bill data is invalid.");

          var existingBill = await _Service_context.GetBillByIdAsync(id);
          if (existingBill == null)
              return NotFound("Bill not found.");

          var modifiedBill = await _Service_context.S_ModifyBillAsync(id, updatedBill);
          return Ok(modifiedBill);
      }



      [HttpPost("Selling_bill_without_gst/{id}")]
      public async Task<IActionResult> Selling_bill_without_gst(int id)

      {

          var bill = await _b1.S_Bills
                                   .Include(b => b.Items)       // Include related Items
                                   .ThenInclude(s => s.Stock)
                                   .ThenInclude(i => i.Material) // Eagerly load Material if it's a navigation property
                                   .FirstOrDefaultAsync(b => b.S_Id == id);

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

          // Assuming _b.GenerateInvoice is a method that genersates the PDF
          _b2.Generate_Invoice_WithOut_Gst(filePath, new List<S_Bill> { bill });

          // Read the PDF file into a byte array
          var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

          // Return the file to the client with proper content type
          return File(fileBytes, "application/pdf", fileName);
      }


  }
        public string GenerateNextBillNumber(bool includeGst)
        {
            string prefix = includeGst ? "GST" : "NON";
            var lastBill = _b1.C_Bills
                .Where(b => b.IncludeGst == includeGst)
                .OrderByDescending(b => includeGst ? b.GstBillNo : b.NonGstBillNo)
                .FirstOrDefault();

            int lastNumber = 0;
            if (lastBill != null)
            {
                string billNo = includeGst ? lastBill.GstBillNo : lastBill.NonGstBillNo;
                if (billNo != null && billNo.Length > 3)
                {
                    int.TryParse(billNo.Substring(3), out lastNumber);
                }
            }

            return $"{prefix}{(lastNumber + 1).ToString("D5")}";
        }
         */
    }

}


