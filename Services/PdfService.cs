using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;

namespace AutoEdge.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateQRCodePdfAsync(byte[] qrCodeImage, string recipientName, string vehicleInfo, string trackingNumber, DateTime scheduledDate);
    }

    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        ////public async Task<byte[]> GenerateQRCodePdfAsync(byte[] qrCodeImage, string recipientName, string vehicleInfo, string trackingNumber, DateTime scheduledDate)
        ////{
        ////    try
        ////    {
        ////        return await Task.Run(() =>
        ////        {
        ////            using var memoryStream = new MemoryStream();
        ////            using var writer = new PdfWriter(memoryStream);
        ////            using var pdf = new PdfDocument(writer);
        ////            using var document = new Document(pdf);

        ////            // Set document properties
        ////            pdf.GetDocumentInfo().SetTitle("Vehicle Delivery QR Code");
        ////            pdf.GetDocumentInfo().SetAuthor("AutoEdge Dealership");
        ////            pdf.GetDocumentInfo().SetSubject($"Delivery QR Code for {vehicleInfo}");

        ////            // Add header
        ////            var header = new Paragraph("AutoEdge Dealership")
        ////                .SetFontSize(24)
        ////                .SetBold()
        ////                .SetTextAlignment(TextAlignment.CENTER)
        ////                .SetFontColor(ColorConstants.BLUE)
        ////                .SetMarginBottom(10);
        ////            document.Add(header);

        ////            var subHeader = new Paragraph("Vehicle Delivery Verification Code")
        ////                .SetFontSize(16)
        ////                .SetTextAlignment(TextAlignment.CENTER)
        ////                .SetMarginBottom(20);
        ////            document.Add(subHeader);

        ////            // Add delivery information
        ////            var infoTable = new Table(2)
        ////                .SetWidth(UnitValue.CreatePercentValue(100))
        ////                .SetMarginBottom(20);

        ////            infoTable.AddCell(CreateInfoCell("Customer Name:", recipientName));
        ////            infoTable.AddCell(CreateInfoCell("Vehicle:", vehicleInfo));
        ////            infoTable.AddCell(CreateInfoCell("Tracking Number:", trackingNumber));
        ////            infoTable.AddCell(CreateInfoCell("Scheduled Date:", scheduledDate.ToString("MMM dd, yyyy HH:mm")));

        ////            document.Add(infoTable);

        ////            // Add QR code section
        ////            var qrSection = new Paragraph("Delivery QR Code")
        ////                .SetFontSize(18)
        ////                .SetBold()
        ////                .SetTextAlignment(TextAlignment.CENTER)
        ////                .SetMarginTop(20)
        ////                .SetMarginBottom(10);
        ////            document.Add(qrSection);

        ////            var qrInstruction = new Paragraph("Present this QR code to the delivery driver for verification:")
        ////                .SetFontSize(12)
        ////                .SetTextAlignment(TextAlignment.CENTER)
        ////                .SetMarginBottom(15);
        ////            document.Add(qrInstruction);

        ////            // Add QR code image
        ////            var qrImageData = ImageDataFactory.Create(qrCodeImage);
        ////            var qrImage = new Image(qrImageData)
        ////                .SetWidth(200)
        ////                .SetHeight(200)
        ////                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
        ////                .SetMarginBottom(15);
        ////            document.Add(qrImage);

        ////            // Add security warning
        ////            var warningBox = new Div()
        ////                .SetBorder(new SolidBorder(ColorConstants.RED, 2))
        ////                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
        ////                .SetPadding(10)
        ////                .SetMarginTop(20);

        ////            var warningTitle = new Paragraph("⚠️ IMPORTANT SECURITY NOTICE")
        ////                .SetFontSize(14)
        ////                .SetBold()
        ////                .SetFontColor(ColorConstants.RED)
        ////                .SetTextAlignment(TextAlignment.CENTER)
        ////                .SetMarginBottom(5);
        ////            warningBox.Add(warningTitle);

        ////            var warningText = new Paragraph("Keep this QR code secure and only share it with the authorized delivery driver. QR Code expires in 30 days from delivery scheduling.")
        ////                .SetFontSize(10)
        ////                .SetTextAlignment(TextAlignment.CENTER);
        ////            warningBox.Add(warningText);

        ////            document.Add(warningBox);

        ////            // Add delivery instructions
        ////            var instructionsTitle = new Paragraph("Delivery Instructions:")
        ////                .SetFontSize(14)
        ////                .SetBold()
        ////                .SetMarginTop(20)
        ////                .SetMarginBottom(10);
        ////            document.Add(instructionsTitle);

        ////            var instructions = new List()
        ////                .SetListSymbol("•")
        ////                .SetMarginLeft(20);

        ////            instructions.Add(new ListItem("Be available at the scheduled delivery time"));
        ////            instructions.Add(new ListItem("Have this QR code ready on your phone or printed"));
        ////            instructions.Add(new ListItem("Verify the driver's identity before showing the QR code"));
        ////            instructions.Add(new ListItem("Inspect the vehicle before accepting delivery"));
        ////            instructions.Add(new ListItem("The driver will scan this QR code to complete the delivery"));

        ////            document.Add(instructions);

        ////            // Add footer
        ////            var footer = new Paragraph("AutoEdge Dealership | 123 Auto Street, City, State 12345 | (555) 123-4567")
        ////                .SetFontSize(10)
        ////                .SetTextAlignment(TextAlignment.CENTER)
        ////                .SetMarginTop(30)
        ////                .SetFontColor(ColorConstants.GRAY);
        ////            document.Add(footer);

        ////            return memoryStream.ToArray();
        ////        });
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        _logger.LogError(ex, "Error generating QR code PDF");
        ////        throw;
        ////    }
        ////}
        public async Task<byte[]> GenerateQRCodePdfAsync(
    byte[] qrCodeImage,
    string recipientName,
    string vehicleInfo,
    string trackingNumber,
    DateTime scheduledDate)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var memoryStream = new MemoryStream();

                    // Create PDF writer/documents
                    using (var writer = new PdfWriter(memoryStream))
                    using (var pdf = new PdfDocument(writer))
                    using (var document = new Document(pdf))
                    {
                        // ... your existing code for header, table, qr, warning, etc.

                        // Add header
                        var header = new Paragraph("AutoEdge Dealership")
                            .SetFontSize(24)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontColor(ColorConstants.BLUE)
                            .SetMarginBottom(10);
                        document.Add(header);

                        var subHeader = new Paragraph("Vehicle Delivery Verification Code")
                            .SetFontSize(16)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(20);
                        document.Add(subHeader);

                        // Add delivery information
                        var infoTable = new Table(2)
                            .SetWidth(UnitValue.CreatePercentValue(100))
                            .SetMarginBottom(20);

                        infoTable.AddCell(CreateInfoCell("Customer Name:", recipientName));
                        infoTable.AddCell(CreateInfoCell("Vehicle:", vehicleInfo));
                        infoTable.AddCell(CreateInfoCell("Tracking Number:", trackingNumber));
                        infoTable.AddCell(CreateInfoCell("Scheduled Date:", scheduledDate.ToString("MMM dd, yyyy HH:mm")));

                        document.Add(infoTable);

                        // Add QR code section
                        var qrSection = new Paragraph("Delivery QR Code")
                            .SetFontSize(18)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginTop(20)
                            .SetMarginBottom(10);
                        document.Add(qrSection);

                        var qrInstruction = new Paragraph("Present this QR code to the delivery driver for verification:")
                            .SetFontSize(12)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(15);
                        document.Add(qrInstruction);

                        // Add QR code image
                        var qrImageData = ImageDataFactory.Create(qrCodeImage);
                        var qrImage = new Image(qrImageData)
                            .SetWidth(200)
                            .SetHeight(200)
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                            .SetMarginBottom(15);
                        document.Add(qrImage);

                        // Add security warning
                        var warningBox = new Div()
                            .SetBorder(new SolidBorder(ColorConstants.RED, 2))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            .SetPadding(10)
                            .SetMarginTop(20);

                        var warningTitle = new Paragraph("⚠️ IMPORTANT SECURITY NOTICE")
                            .SetFontSize(14)
                            .SetBold()
                            .SetFontColor(ColorConstants.RED)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(5);
                        warningBox.Add(warningTitle);

                        var warningText = new Paragraph("Keep this QR code secure and only share it with the authorized delivery driver. QR Code expires in 30 days from delivery scheduling.")
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.CENTER);
                        warningBox.Add(warningText);

                        document.Add(warningBox);

                        // Add delivery instructions
                        var instructionsTitle = new Paragraph("Delivery Instructions:")
                            .SetFontSize(14)
                            .SetBold()
                            .SetMarginTop(20)
                            .SetMarginBottom(10);
                        document.Add(instructionsTitle);

                        var instructions = new List()
                            .SetListSymbol("•")
                            .SetMarginLeft(20);

                        instructions.Add(new ListItem("Be available at the scheduled delivery time"));
                        instructions.Add(new ListItem("Have this QR code ready on your phone or printed"));
                        instructions.Add(new ListItem("Verify the driver's identity before showing the QR code"));
                        instructions.Add(new ListItem("Inspect the vehicle before accepting delivery"));
                        instructions.Add(new ListItem("The driver will scan this QR code to complete the delivery"));

                        document.Add(instructions);

                        // Footer
                        var footer = new Paragraph("AutoEdge Dealership | 123 Auto Street, City, State 12345 | (2764) 123-4567")
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginTop(30)
                            .SetFontColor(ColorConstants.GRAY);
                        document.Add(footer);
                    } // <- disposal happens here (document/pdf/writer all flushed)

                    // Now safe to read bytes
                    return memoryStream.ToArray();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code PDF");
                throw;
            }
        }



        private Cell CreateInfoCell(string label, string value)
        {
            var cell = new Cell();
            
            var labelParagraph = new Paragraph(label)
                .SetBold()
                .SetFontSize(12)
                .SetMarginBottom(2);
            
            var valueParagraph = new Paragraph(value)
                .SetFontSize(12);
            
            cell.Add(labelParagraph);
            cell.Add(valueParagraph);
            cell.SetPadding(8);
            cell.SetBorder(Border.NO_BORDER);
            
            return cell;
        }

    }
}