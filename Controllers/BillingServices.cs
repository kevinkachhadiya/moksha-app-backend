using iTextSharp.text.pdf;
using iTextSharp.text;
using MAPI.Models;
using Microsoft.AspNetCore.Mvc;

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
                using (var fileStream = new FileStream(fileName, FileMode.Create))
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
                    PdfPCell companyDetails = new PdfPCell(new Phrase("MOKSHA PLASTIC\nUrmi Indi, Estate, Plot No. 6, Ved Road\nSurat, Gujarat, 395004\nPhone: (+91) 98790-02296\nEmail: sudhir@kachchadiya@gmail.com", FontFactory.GetFont("Arial", 12, Font.NORMAL)))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    headerTable.AddCell(companyDetails);

                    // Add the header table to the document
                    document.Add(headerTable);

                    // Add invoice title
                    var title = new Paragraph();
                
                    var chunk2 = new Chunk("BUYING BILLING", FontFactory.GetFont("Arial", 16, Font.BOLD));

                   
                    title.Add(chunk2);

                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingBefore = 10;
                    title.SpacingAfter = 10;

                    document.Add(title);

                    // Initialize the details table
                    PdfPTable details = new PdfPTable(2);
                    details.WidthPercentage = 100;
                    details.SetWidths(new float[] { 2.8f, 2.2f }); // Adjust column widths to fit content
                    // Add the details table to the document
                    document.Add(details);

                    // Add customer details
                    var customerDetails = new Paragraph($"NAME : {bills[0].BuyerName}\nBILL NO : {bills[0].BillNo}\nINVOICE DATE: {bills[0].CreatedAt:dd/MM/yyyy HH:mm:ss}", FontFactory.GetFont("Arial", 12, Font.NORMAL))
                    {
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };
                    document.Add(customerDetails);



                    // Add items in the bill (styled table)
                    PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { .50F, 1.15F, 1.10F, 1.10F, 1.15F }); // Adjusted widths for better readability

                    // Add table header
                    AddTableHeader(table);

                    // Add table rows with alternating colors
                    bool isGray = false;
                    decimal totalAmount = 0; // Total amount for all items

                    var ser_no = 1;
                    // Loop through the items to add rows
                    foreach (var item in bills[0].Items)
                    {
                        if (item != null)
                        {
                            BaseColor rowColor = isGray ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                            isGray = !isGray;
                            Font font = FontFactory.GetFont("Helvetica", 12, Font.NORMAL);

                            table.AddCell(new PdfPCell(new Phrase(ser_no.ToString())) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(item.ColorName)) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString())) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase($" ₹{item.Price.ToString("0.00")}", font)) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase($" ₹{item.TotalPrice.ToString("0.00")}")) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER });

                            totalAmount += item.TotalPrice;
                            ser_no++;
                        }
                        else
                        {

                        }
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
                    }

                    document.Add(table);

                    // Add total price in INR (Rupees)
                    var totalAmountParagraph = new Paragraph($"Total Amount: ₹ {totalAmount.ToString("0.00")}", FontFactory.GetFont("Helvetica", 12, Font.BOLD))
                    {
                        Alignment = Element.ALIGN_RIGHT,
                        SpacingBefore = 10
                    };
                    document.Add(totalAmountParagraph);
                    var modepayment = new Paragraph($"Mode of Payment: {bills[0].PaymentMethod}", FontFactory.GetFont("Helvetica", 12, Font.BOLD))
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
                    var footer = new Paragraph("Thank you for your business!\n  Contact us for any queries: sudhirkachchadiya@gmail.com", FontFactory.GetFont("Arial", 10, Font.ITALIC))
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 15
                    };
                    document.Add(footer);

                    document.Close();
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
                table.AddCell(new PdfPCell(new Phrase("Quantity(KG)", FontFactory.GetFont("Arial", 12, Font.BOLD)))
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
