using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Image;
using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IQRCodePdfService
    {
        Task<byte[]> GenerateServiceBookingQRCodePdfAsync(ServiceBooking booking, byte[] qrCodeImage);
    }

    public class QRCodePdfService : IQRCodePdfService
    {
        private readonly ILogger<QRCodePdfService> _logger;

        public QRCodePdfService(ILogger<QRCodePdfService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> GenerateServiceBookingQRCodePdfAsync(ServiceBooking booking, byte[] qrCodeImage)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new PdfWriter(memoryStream);
                using var pdf = new PdfDocument(writer);
                using var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

                // Create header
                var header = new Paragraph("AUTOEDGE SERVICE CENTER")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(24)
                    .SetBold()
                    .SetMarginBottom(10);

                var subHeader = new Paragraph("Service Booking QR Code Ticket")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetMarginBottom(20);

                document.Add(header);
                document.Add(subHeader);

                // Create main content table
                var table = new Table(2).UseAllAvailableWidth();

                // Left column - QR Code
                var qrCodeCell = new Cell();
                qrCodeCell.SetPadding(20);
                qrCodeCell.SetTextAlignment(TextAlignment.CENTER);

                // Add QR Code image
                var qrCodeImageData = ImageDataFactory.Create(qrCodeImage);
                var qrCodeImg = new Image(qrCodeImageData);
                qrCodeImg.ScaleToFit(200, 200);
                qrCodeImg.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                qrCodeCell.Add(qrCodeImg);

                // Add QR Code instructions
                var qrInstructions = new Paragraph("SCAN THIS QR CODE")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14)
                    .SetBold()
                    .SetMarginTop(10);

                qrCodeCell.Add(qrInstructions);

                var instructionText = new Paragraph("Present this ticket at the service center")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetMarginTop(5);

                qrCodeCell.Add(instructionText);

                // Right column - Booking Details
                var detailsCell = new Cell();
                detailsCell.SetPadding(20);
                detailsCell.SetVerticalAlignment(VerticalAlignment.TOP);

                // Booking Reference (large and prominent)
                var refHeader = new Paragraph("BOOKING REFERENCE")
                    .SetFontSize(12)
                    .SetBold()
                    .SetMarginBottom(5);

                var refNumber = new Paragraph(booking.Reference)
                    .SetFontSize(18)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(15)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetPadding(10);

                detailsCell.Add(refHeader);
                detailsCell.Add(refNumber);

                // Customer Information
                var customerHeader = new Paragraph("CUSTOMER INFORMATION")
                    .SetFontSize(12)
                    .SetBold()
                    .SetMarginBottom(5);

                var customerDetails = new Paragraph()
                    .Add($"Name: {booking.CustomerName}\n")
                    .Add($"Email: {booking.CustomerEmail}\n")
                    .Add($"Phone: {booking.CustomerPhone ?? "N/A"}")
                    .SetFontSize(10)
                    .SetMarginBottom(10);

                detailsCell.Add(customerHeader);
                detailsCell.Add(customerDetails);

                // Vehicle Information
                var vehicleHeader = new Paragraph("VEHICLE INFORMATION")
                    .SetFontSize(12)
                    .SetBold()
                    .SetMarginBottom(5);

                var vehicleDetails = new Paragraph()
                    .Add($"Vehicle: {booking.Make} {booking.Model} ({booking.Year})\n")
                    .Add($"VIN: {booking.VIN ?? "N/A"}\n")
                    .Add($"Mileage: {booking.Mileage?.ToString() ?? "N/A"}")
                    .SetFontSize(10)
                    .SetMarginBottom(10);

                detailsCell.Add(vehicleHeader);
                detailsCell.Add(vehicleDetails);

                // Service Information
                var serviceHeader = new Paragraph("SERVICE INFORMATION")
                    .SetFontSize(12)
                    .SetBold()
                    .SetMarginBottom(5);

                var serviceDetails = new Paragraph()
                    .Add($"Service Type: {booking.ServiceType}\n")
                    .Add($"Scheduled Date: {booking.PreferredDate:MMMM dd, yyyy}\n")
                    .Add($"Scheduled Time: {booking.PreferredStart:HH:mm}\n")
                    .Add($"Estimated Duration: {booking.EstimatedDurationMin} minutes\n")
                    .Add($"Estimated Cost: ${booking.EstimatedCost:F2}\n")
                    .Add($"Delivery Method: {booking.DeliveryMethod}")
                    .SetFontSize(10)
                    .SetMarginBottom(10);

                detailsCell.Add(serviceHeader);
                detailsCell.Add(serviceDetails);

                // Add cells to table
                table.AddCell(qrCodeCell);
                table.AddCell(detailsCell);

                document.Add(table);

                // Add footer information
                var footer = new Paragraph()
                    .Add("IMPORTANT INSTRUCTIONS:\n")
                    .Add("• Present this ticket at the service center reception\n")
                    .Add("• Keep this ticket safe - it contains your booking reference\n")
                    .Add("• QR code expires in 7 days from booking date\n")
                    .Add("• Bring vehicle registration and insurance documents\n")
                    .Add("• Remove personal belongings from your vehicle")
                    .SetFontSize(10)
                    .SetMarginTop(20)
                    .SetPadding(15)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                document.Add(footer);

                // Add contact information
                var contactInfo = new Paragraph()
                    .Add("AutoEdge Service Center | 123 Auto Street, City, State 12345 | (555) 123-4567")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(8)
                    .SetMarginTop(10);

                document.Add(contactInfo);

                document.Close();

                var pdfBytes = memoryStream.ToArray();
                _logger.LogInformation("Generated QR code PDF with {Size} bytes for booking {BookingId}", pdfBytes.Length, booking.Id);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code PDF for booking {BookingId}", booking.Id);
                throw;
            }
        }
    }
}
