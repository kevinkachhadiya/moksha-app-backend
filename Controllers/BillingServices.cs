using iTextSharp.text.pdf;
using iTextSharp.text;
using MAPI.Models;
using Microsoft.AspNetCore.Mvc;
using iTextSharp.text.pdf.draw;

namespace MAPI.Controllers
{
    public class BillingServices : ControllerBase
    {
 
        public BillingServices()
        {
        }

        public void GenerateInvoice(string fileName, List<B_Bill> bills)
        {
            // Validate bills input
            if (bills == null || bills.Count == 0)
            {
                throw new ArgumentException("Bills list is null or empty");
            }

            // Get the first bill for invoice details
            var bill = bills[0];

            // Create a new Document with A4 size for better readability
            Document document = new Document(PageSize.A4, 36, 36, 50, 36);

            try
            {
                // Ensure directory exists
                string directoryPath = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Create PDF writer and document
                using (var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, fileStream);

                    // Add page numbers to footer
                    writer.PageEvent = new PageNumberFooter();

                    document.Open();

                    // Add styled header
                    AddHeaderSection(document, bill);

                    // Add invoice information
                    AddInvoiceInfoSection(document, bill);

                    // Add items table
                    AddItemsTable(document, bill);

                    // Add payment summary
                    AddPaymentSummary(document, bill);

                    // Add terms and signature section
                    AddTermsAndSignature(document);

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating invoice: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adds the company header with logo and contact details
        /// </summary>
        private void AddHeaderSection(Document document, B_Bill bill)
        {
            // Create header table
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 1, 3 });

            try
            {
                // Logo cell
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80, 80);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingTop = 5,
                    PaddingBottom = 5,
                    PaddingLeft = 5,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                headerTable.AddCell(logoCell);

                // Company details cell
                Font companyFont = FontFactory.GetFont("Arial", 11, Font.NORMAL);
                Font companyNameFont = FontFactory.GetFont("Arial", 16, Font.BOLD);

                Paragraph companyInfo = new Paragraph();
                companyInfo.Add(new Chunk("MOKSHA PLASTIC\n", companyNameFont));
                companyInfo.Add(new Chunk("Urmi Indi, Estate, Plot No. 6, Ved Road\n", companyFont));
                companyInfo.Add(new Chunk("Surat, Gujarat, 395004\n", companyFont));
                companyInfo.Add(new Chunk("Phone: (+91) 98790-02296\n", companyFont));
                companyInfo.Add(new Chunk("Email: sudhir@kachchadiya@gmail.com", companyFont));

                PdfPCell companyCell = new PdfPCell(companyInfo)
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingTop = 5,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                headerTable.AddCell(companyCell);

                // Add header table to document
                document.Add(headerTable);

                // Add separator line
                LineSeparator line = new LineSeparator(1f, 100f, new BaseColor(210, 210, 210), Element.ALIGN_CENTER, -10);
                document.Add(line);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding header: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds invoice title and customer information
        /// </summary>
        private void AddInvoiceInfoSection(Document document, B_Bill bill)
        {
            try
            {
                // Add invoice title
                Paragraph title = new Paragraph();
                title.Add(new Chunk("PURCHASE INVOICE", FontFactory.GetFont("Arial", 18, Font.BOLD)));
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingBefore = 15;
                title.SpacingAfter = 15;
                document.Add(title);

                // Create a table for invoice details and customer info
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 1, 1 });

                // Left side - Customer details
                Font labelFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
                Font valueFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);

                Paragraph customerInfo = new Paragraph();
                customerInfo.Add(new Chunk("BUYER DETAILS\n\n", FontFactory.GetFont("Arial", 12, Font.BOLD)));
                customerInfo.Add(new Chunk("Name: ", labelFont));
                customerInfo.Add(new Chunk(bill.BuyerName + "\n", valueFont));

                PdfPCell customerCell = new PdfPCell(customerInfo);
                customerCell.Border = Rectangle.NO_BORDER;
                customerCell.PaddingBottom = 10;
                infoTable.AddCell(customerCell);

                // Right side - Invoice details
                Paragraph invoiceInfo = new Paragraph();
                invoiceInfo.Add(new Chunk("INVOICE DETAILS\n\n", FontFactory.GetFont("Arial", 12, Font.BOLD)));
                invoiceInfo.Add(new Chunk("Invoice No: ", labelFont));
                invoiceInfo.Add(new Chunk(bill.BillNo + "\n", valueFont));
                invoiceInfo.Add(new Chunk("Date: ", labelFont));
                invoiceInfo.Add(new Chunk(bill.CreatedAt.ToString("dd MMM yyyy") + "\n", valueFont));
                invoiceInfo.Add(new Chunk("Time: ", labelFont));
                invoiceInfo.Add(new Chunk(bill.CreatedAt.ToString("hh:mm tt") + "\n", valueFont));

                PdfPCell invoiceCell = new PdfPCell(invoiceInfo);
                invoiceCell.Border = Rectangle.NO_BORDER;
                invoiceCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                infoTable.AddCell(invoiceCell);

                document.Add(infoTable);

                // Add space after info table
                document.Add(new Paragraph(" "));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding invoice info: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds the items table showing purchased materials
        /// </summary>
        private void AddItemsTable(Document document, B_Bill bill)
        {
            try
            {
                // Create table for items
                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 0.5f, 2f, 1f, 1f, 1.5f });
                table.SpacingBefore = 10;
                table.SpacingAfter = 10;

                // Table header style
                Font headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
                BaseColor headerBackground = new BaseColor(41, 128, 185); // Professional blue color

                // Add header cells
                AddHeaderCell(table, "S.No", headerFont, headerBackground);
                AddHeaderCell(table, "Material", headerFont, headerBackground);
                AddHeaderCell(table, "Quantity (KG)", headerFont, headerBackground);
                AddHeaderCell(table, "Rate (₹)", headerFont, headerBackground);
                AddHeaderCell(table, "Amount (₹)", headerFont, headerBackground);

                // Regular cell style
                Font cellFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);
                BaseColor lightRowColor = new BaseColor(245, 245, 245);
                BaseColor darkRowColor = BaseColor.WHITE;

                // Track total amount
                decimal totalAmount = 0;

                // Add item rows with alternating colors
                for (int i = 0; i < Math.Max(bill.Items.Count, 10); i++)
                {
                    BaseColor rowColor = (i % 2 == 0) ? lightRowColor : darkRowColor;

                    // If we have actual data, use it
                    if (i < bill.Items.Count && bill.Items[i] != null)
                    {
                        var item = bill.Items[i];

                        AddCell(table, (i + 1).ToString(), cellFont, rowColor, Element.ALIGN_CENTER);
                        AddCell(table, item.ColorName, cellFont, rowColor, Element.ALIGN_LEFT);
                        AddCell(table, item.Quantity.ToString("0.00"), cellFont, rowColor, Element.ALIGN_RIGHT);
                        AddCell(table, item.Price.ToString("0.00"), cellFont, rowColor, Element.ALIGN_RIGHT);
                        AddCell(table, item.TotalPrice.ToString("0.00"), cellFont, rowColor, Element.ALIGN_RIGHT);

                        totalAmount += item.TotalPrice;
                    }
                    // Otherwise add empty rows to ensure minimum size
                    else
                    {
                        AddCell(table, (i + 1).ToString(), cellFont, rowColor, Element.ALIGN_CENTER);
                        AddCell(table, "", cellFont, rowColor, Element.ALIGN_LEFT);
                        AddCell(table, "", cellFont, rowColor, Element.ALIGN_RIGHT);
                        AddCell(table, "", cellFont, rowColor, Element.ALIGN_RIGHT);
                        AddCell(table, "", cellFont, rowColor, Element.ALIGN_RIGHT);
                    }
                }

                // Add the table to the document
                document.Add(table);

                // Store the total amount for later use
                document.Add(new Paragraph("", FontFactory.GetFont("Arial", 6))); // Small spacing

                // Add total information
                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 50;
                totalTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalTable.SetWidths(new float[] { 1, 1 });

                // Total row
                PdfPCell totalLabelCell = new PdfPCell(new Phrase("Total Amount:", FontFactory.GetFont("Arial", 11, Font.BOLD)));
                totalLabelCell.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER;
                totalLabelCell.BackgroundColor = new BaseColor(240, 240, 240);
                totalLabelCell.HorizontalAlignment = Element.ALIGN_LEFT;
                totalLabelCell.Padding = 5;
                totalTable.AddCell(totalLabelCell);

                PdfPCell totalValueCell = new PdfPCell(new Phrase("₹ " + totalAmount.ToString("0.00"), FontFactory.GetFont("Arial", 11, Font.BOLD)));
                totalValueCell.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER;
                totalValueCell.BackgroundColor = new BaseColor(240, 240, 240);
                totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalValueCell.Padding = 5;
                totalTable.AddCell(totalValueCell);

                document.Add(totalTable);

                // Add amount in words
                Paragraph amountInWords = new Paragraph();
                amountInWords.Add(new Chunk("Amount in words: ", FontFactory.GetFont("Arial", 10, Font.BOLD)));
                amountInWords.Add(new Chunk(ConvertToWords(totalAmount) + " rupees only", FontFactory.GetFont("Arial", 10, Font.ITALIC)));
                amountInWords.SpacingBefore = 10;
                amountInWords.SpacingAfter = 10;
                document.Add(amountInWords);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding items table: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds payment details and summary
        /// </summary>
        private void AddPaymentSummary(Document document, B_Bill bill)
        {
            try
            {
                PdfPTable paymentTable = new PdfPTable(2);
                paymentTable.WidthPercentage = 100;
                paymentTable.SetWidths(new float[] { 1, 1 });

                // Payment method
                Paragraph paymentDetails = new Paragraph();
                paymentDetails.Add(new Chunk("PAYMENT DETAILS\n\n", FontFactory.GetFont("Arial", 12, Font.BOLD)));
                paymentDetails.Add(new Chunk("Method: ", FontFactory.GetFont("Arial", 10, Font.BOLD)));
                paymentDetails.Add(new Chunk(bill.PaymentMethod.ToString(), FontFactory.GetFont("Arial", 10)));

                PdfPCell paymentCell = new PdfPCell(paymentDetails);
                paymentCell.Border = Rectangle.NO_BORDER;
                paymentTable.AddCell(paymentCell);

                // For authorized signatory
                Paragraph signatory = new Paragraph();
                signatory.Add(new Chunk("FOR MOKSHA PLASTIC\n\n\n\n", FontFactory.GetFont("Arial", 12, Font.BOLD)));
                signatory.Add(new Chunk("Authorized Signatory", FontFactory.GetFont("Arial", 10)));
                signatory.Alignment = Element.ALIGN_RIGHT;

                PdfPCell signatoryCell = new PdfPCell(signatory);
                signatoryCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                signatoryCell.Border = Rectangle.NO_BORDER;
                paymentTable.AddCell(signatoryCell);

                document.Add(paymentTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding payment summary: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds terms and conditions and final footer
        /// </summary>
        private void AddTermsAndSignature(Document document)
        {
            try
            {
                // Add separator line
                LineSeparator line = new LineSeparator(1f, 100f, new BaseColor(210, 210, 210), Element.ALIGN_CENTER, -5);
                document.Add(line);

                // Terms and conditions
                Paragraph terms = new Paragraph();
                terms.Add(new Chunk("Terms & Conditions:\n", FontFactory.GetFont("Arial", 10, Font.BOLD)));
                terms.Add(new Chunk("1. Goods once sold will not be taken back.\n", FontFactory.GetFont("Arial", 8)));
                terms.Add(new Chunk("2. Interest @18% p.a. will be charged if payment is not made within the stipulated time.\n", FontFactory.GetFont("Arial", 8)));
                terms.Add(new Chunk("3. All disputes are subject to Surat jurisdiction only.", FontFactory.GetFont("Arial", 8)));
                terms.SpacingBefore = 10;
                terms.SpacingAfter = 10;
                document.Add(terms);

                // Final thank you note
                Paragraph footer = new Paragraph("Thank you for your business!", FontFactory.GetFont("Arial", 10, Font.ITALIC));
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 5;
                document.Add(footer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding terms and signature: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to add styled header cells to the table
        /// </summary>
        private void AddHeaderCell(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = backgroundColor;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 8;
            table.AddCell(cell);
        }

        /// <summary>
        /// Helper method to add styled data cells to the table
        /// </summary>
        private void AddCell(PdfPTable table, string text, Font font, BaseColor backgroundColor, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = backgroundColor;
            cell.HorizontalAlignment = alignment;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 6;
            table.AddCell(cell);
        }

        /// <summary>
        /// Custom PageEvent class to add page numbers to the footer
        /// </summary>
        private class PageNumberFooter : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfContentByte cb = writer.DirectContent;
                Font font = FontFactory.GetFont("Arial", 8, Font.NORMAL);

                string text = "Page " + writer.PageNumber + " | Generated on " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                float textWidth = font.GetCalculatedBaseFont(true).GetWidthPoint(text, 8);
                float textBase = document.Bottom - 20;

                cb.BeginText();
                cb.SetFontAndSize(font.GetCalculatedBaseFont(true), 8);
                cb.SetTextMatrix(document.Right - textWidth, textBase);
                cb.ShowText(text);
                cb.EndText();

                // Add contact info on left side
                string contactText = "For inquiries: sudhirkachchadiya@gmail.com | (+91) 98790-02296";
                cb.BeginText();
                cb.SetFontAndSize(font.GetCalculatedBaseFont(true), 8);
                cb.SetTextMatrix(document.Left, textBase);
                cb.ShowText(contactText);
                cb.EndText();
            }
        }

        /// <summary>
        /// Converts a numeric amount to words (Indian numbering system)
        /// </summary>
        private string ConvertToWords(decimal amount)
        {
            if (amount == 0) return "Zero";

            string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            // Split amount into rupees and paise
            int rupees = (int)Math.Floor(amount);
            int paise = (int)Math.Round((amount - rupees) * 100);

            string result = "";

            if (rupees > 0)
            {
                // Convert crores
                if (rupees > 9999999)
                {
                    int crore = rupees / 10000000;
                    result += ConvertNumberToWords(crore) + " Crore ";
                    rupees %= 10000000;
                }

                // Convert lakhs
                if (rupees > 99999)
                {
                    int lakh = rupees / 100000;
                    result += ConvertNumberToWords(lakh) + " Lakh ";
                    rupees %= 100000;
                }

                // Convert thousands
                if (rupees > 999)
                {
                    int thousand = rupees / 1000;
                    result += ConvertNumberToWords(thousand) + " Thousand ";
                    rupees %= 1000;
                }

                // Convert hundreds
                if (rupees > 99)
                {
                    int hundred = rupees / 100;
                    result += ConvertNumberToWords(hundred) + " Hundred ";
                    rupees %= 100;
                }

                // Convert tens and units
                if (rupees > 0)
                {
                    result += ConvertNumberToWords(rupees);
                }
            }

            // Add paise if applicable
            if (paise > 0)
            {
                result += " and " + ConvertNumberToWords(paise) + " Paise";
            }

            return result.Trim();

            // Local function to convert a number less than 100 to words
            string ConvertNumberToWords(int number)
            {
                if (number < 20)
                    return units[number];

                return tens[number / 10] + (number % 10 > 0 ? " " + units[number % 10] : "");
            }
        }

        public void Generate_Invoice_WithOut_Gst(string fileName, List<S_Bill> bills)
        {
            // Validate bills input
            if (bills == null || bills.Count == 0)
            {
                throw new ArgumentException("Bills list is null or empty");
            }

            // Create a new Document
            Document document = new Document(PageSize.A5, 30, 30, 30, 30);

            try
            {
                string directoryPath = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Use a FileStream to create the PDF file
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    if (document != null && fileStream != null)
                    {
                        // Create PdfWriter before opening the document
                        PdfWriter writer = PdfWriter.GetInstance(document, fileStream);

                        // Open the document after initializing the PdfWriter
                        document.Open();

                        // Initialize the header table
                        PdfPTable headerTable = new PdfPTable(2);
                        headerTable.WidthPercentage = 100;
                        headerTable.SetWidths(new float[] { 1, 4 });

                        // Add logo to the table
                        string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                        Image logo = Image.GetInstance(logoPath);
                        logo.ScaleToFit(60, 60);
                        logo.Alignment = Element.ALIGN_LEFT;
                        headerTable.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER });

                        // Add company details to the table
                        PdfPCell companyDetails = new PdfPCell(new Phrase("MOKSHA PLASTIC\nUrmi Indi,Estate, Plot No. 6, Ved Road\nSurat, Gujrat, 395004\nPhone: (+91) 98790-02296\nEmail: sudhirkachchadiya@gmail.com", FontFactory.GetFont("Arial", 12, Font.NORMAL)))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        headerTable.AddCell(companyDetails);

                        document.Add(headerTable);

                        // Add invoice title
                        var title = new Paragraph("SELLING INVOICE", FontFactory.GetFont("Arial", 16, Font.BOLD))
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingBefore = 10,
                            SpacingAfter = 10
                        };
                        document.Add(title);

                        // Add customer details
                        var customerDetails = new Paragraph($"NAME : {bills[0].SellerName}\nBILL NO : {bills[0].S_BillNo}\nINVOICE DATE : {bills[0].CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")}", FontFactory.GetFont("Arial", 12, Font.NORMAL))
                        {
                            SpacingBefore = 10,
                            SpacingAfter = 10
                        };
                        document.Add(customerDetails);

                        // Initialize the table for bill items
                        PdfPTable table = new PdfPTable(6);
                        table.WidthPercentage = 100;
                        table.SetWidths(new float[] { .50F, 1.10F, 1.10F, 1.10F, 1.05F,1.15F });

                        // Add table header
                        AddTableHeader(table);

                        // Add bill items to the table
                        var ser_no = 1;
                        decimal totalAmount = 0;
                        bool isGray = false;

                        // Loop through the items to add rows
                        foreach (var item in bills[0].Items)
                        {
                            
          
                                BaseColor rowColor = isGray ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                                isGray = !isGray;
                                Font font = FontFactory.GetFont("Helvetica", 12, Font.NORMAL); 

                                table.AddCell(new PdfPCell(new Phrase(ser_no.ToString(), FontFactory.GetFont("Arial", 12, Font.NORMAL))) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_CENTER });
                                table.AddCell(new PdfPCell(new Phrase(item.Stock.Material.ColorName.ToString(), FontFactory.GetFont("Arial", 12, Font.NORMAL))) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_CENTER });
                                table.AddCell(new PdfPCell(new Phrase(item.St_Bags.ToString("0") + "*"+ item.St_Weight.ToString("0"), FontFactory.GetFont("Arial", 12, Font.NORMAL))) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_CENTER });
                                table.AddCell(new PdfPCell(new Phrase(item.S_totalWeight.ToString("0.00"), FontFactory.GetFont("Arial", 12, Font.NORMAL))) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_CENTER });
                                table.AddCell(new PdfPCell(new Phrase(item.price.ToString("0.00"), FontFactory.GetFont("Arial", 12, Font.NORMAL))) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_CENTER });
                                table.AddCell(new PdfPCell(new Phrase((item.S_totalWeight * item.price).ToString("0.00"), FontFactory.GetFont("Arial", 12, Font.NORMAL))) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_CENTER });

                                totalAmount += item.S_totalWeight * item.price;
                                ser_no++;
                                

                        }

                      
                        // Ensure at least 16 rows
                        int rowCount = bills[0].Items.Count;
                        for (int i = rowCount + 1; i <= 10; i++)
                        {

                            BaseColor rowColor = isGray ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                            isGray = !isGray;
                            table.AddCell(new PdfPCell(new Phrase(i.ToString())) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(" ")) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(" ")) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(" ")) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(" ")) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(" ")) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                        }

                        document.Add(table);



                        // Add total price in INR (Rupees)
                        var totalAmountParagraph = new Paragraph($"Total Amount : ₹ {totalAmount.ToString("0.00")}", FontFactory.GetFont("Helvetica", 12, Font.BOLD))
                        {
                            Alignment = Element.ALIGN_RIGHT,
                            SpacingBefore = 10
                        };
                        document.Add(totalAmountParagraph);

                        var modepayment = new Paragraph($"Mode of Payment : {bills[0].PaymentMethod}", FontFactory.GetFont("Helvetica", 12, Font.BOLD))
                        {
                            Alignment = Element.ALIGN_RIGHT,
                            SpacingBefore = 10
                        };
                        document.Add(modepayment);

                        var sig = new Paragraph($"Signature", FontFactory.GetFont("Helvetica", 12, Font.BOLD))
                        {
                            Alignment = Element.ALIGN_RIGHT,
                            SpacingBefore = 20
                        };
                        document.Add(sig);

                        // Add footer
                        var footer = new Paragraph("Thank you for your business!\nContact us for any queries: sudhirkachchadiya@gmail.com", FontFactory.GetFont("Arial", 10, Font.ITALIC))
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingBefore = 15
                        };
                        document.Add(footer);

                        document.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating invoice: {ex.Message}");
                throw;
            }

            // Helper method to add table header
            void AddTableHeader(PdfPTable table)
            {
                {
                    table.AddCell(new PdfPCell(new Phrase("S_NO", FontFactory.GetFont("Arial", 9, Font.BOLD)))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                    table.AddCell(new PdfPCell(new Phrase("Material", FontFactory.GetFont("Arial", 12, Font.BOLD)))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                    table.AddCell(new PdfPCell(new Phrase("bag*weight", FontFactory.GetFont("Arial", 10, Font.BOLD)))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                    table.AddCell(new PdfPCell(new Phrase("Quantity(KG)", FontFactory.GetFont("Arial", 10, Font.BOLD)))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                    table.AddCell(new PdfPCell(new Phrase("Rate", FontFactory.GetFont("Arial", 12, Font.BOLD)))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                    table.AddCell(new PdfPCell(new Phrase("Total Amount", FontFactory.GetFont("Arial", 12, Font.BOLD)))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });


                }
            }
        }



    }
}
