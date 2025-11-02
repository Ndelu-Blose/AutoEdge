using AutoEdge.Models.Entities;
using AutoEdge.Repositories;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoEdge.Services
{
    public class ContractService : IContractService
    {
        private readonly ILogger<ContractService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public ContractService(
            ILogger<ContractService> logger,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        public async Task<ContractGenerationResult> GenerateContractAsync(Purchase purchase)
        {
            try
            {
                // Load purchase with related entities
                var purchases = await _unitOfWork.Purchases
                    .GetWithIncludeAsync(p => p.Id == purchase.Id, null, "Customer.User,Vehicle");
                var fullPurchase = purchases.FirstOrDefault();

                if (fullPurchase == null)
                {
                    return new ContractGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "Purchase not found"
                    };
                }

                // Get contract template
                var template = await GetContractTemplateAsync("VehiclePurchaseContract");
                
                // Process template with purchase data
                var contractHtml = await ProcessTemplateAsync(template, fullPurchase);
                
                // Generate PDF
                var pdfBytes = await GeneratePdfAsync(contractHtml);
                
                // Generate contract number
                var contractNumber = GenerateContractNumber(fullPurchase);

                return new ContractGenerationResult
                {
                    Success = true,
                    ContractHtml = contractHtml,
                    PdfBytes = pdfBytes,
                    ContractNumber = contractNumber,
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating contract for purchase {PurchaseId}", purchase.Id);
                return new ContractGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Contract generation failed: {ex.Message}"
                };
            }
        }

        public async Task<byte[]> GeneratePdfAsync(string contractHtml)
        {
            try
            {
                // For now, we'll simulate PDF generation
                // In a real implementation, you would use a library like iTextSharp, PuppeteerSharp, or wkhtmltopdf
                await Task.Delay(1000); // Simulate PDF generation time
                
                // Convert HTML to bytes (simplified simulation)
                var htmlBytes = Encoding.UTF8.GetBytes(contractHtml);
                
                // In a real implementation, this would be actual PDF bytes
                return htmlBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from HTML");
                throw;
            }
        }

        public async Task<string> GetContractTemplateAsync(string templateName)
        {
            try
            {
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "Contracts", $"{templateName}.html");
                
                if (File.Exists(templatePath))
                {
                    return await File.ReadAllTextAsync(templatePath);
                }
                
                // Return default template if file doesn't exist
                return GetDefaultContractTemplate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contract template {TemplateName}", templateName);
                return GetDefaultContractTemplate();
            }
        }

        public async Task<string> ProcessTemplateAsync(string template, Purchase purchase)
        {
            await Task.Delay(100); // Simulate processing time
            
            var processedTemplate = template;
            
            // Replace customer placeholders
            processedTemplate = processedTemplate.Replace("{{CustomerName}}", purchase.Customer?.User?.FirstName + " " + purchase.Customer?.User?.LastName ?? "N/A");
            processedTemplate = processedTemplate.Replace("{{CustomerEmail}}", purchase.Customer?.User?.Email ?? "N/A");
            processedTemplate = processedTemplate.Replace("{{CustomerPhone}}", purchase.Customer?.User?.PhoneNumber ?? "N/A");
            processedTemplate = processedTemplate.Replace("{{CustomerAddress}}", purchase.Customer?.DeliveryAddress ?? "N/A");
            
            // Replace vehicle placeholders
            processedTemplate = processedTemplate.Replace("{{VehicleMake}}", purchase.Vehicle?.Make ?? "N/A");
            processedTemplate = processedTemplate.Replace("{{VehicleModel}}", purchase.Vehicle?.Model ?? "N/A");
            processedTemplate = processedTemplate.Replace("{{VehicleYear}}", purchase.Vehicle?.Year.ToString() ?? "N/A");
            processedTemplate = processedTemplate.Replace("{{VehicleVIN}}", purchase.Vehicle?.VIN ?? "N/A");
            processedTemplate = processedTemplate.Replace("{{VehiclePrice}}", purchase.Vehicle?.SellingPrice.ToString("C") ?? "N/A");
            processedTemplate = processedTemplate.Replace("{{VehicleMileage}}", purchase.Vehicle?.Mileage.ToString("N0") ?? "N/A");
            
            // Replace purchase placeholders
            processedTemplate = processedTemplate.Replace("{{PurchaseId}}", purchase.Id.ToString());
            processedTemplate = processedTemplate.Replace("{{PurchaseDate}}", purchase.CreatedDate.ToString("MMMM dd, yyyy"));
            processedTemplate = processedTemplate.Replace("{{ContractNumber}}", GenerateContractNumber(purchase));
            processedTemplate = processedTemplate.Replace("{{TotalAmount}}", purchase.Vehicle?.SellingPrice.ToString("C") ?? "N/A");
            
            // Replace date placeholders
            processedTemplate = processedTemplate.Replace("{{CurrentDate}}", DateTime.Now.ToString("MMMM dd, yyyy"));
            processedTemplate = processedTemplate.Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());
            
            return processedTemplate;
        }

        private string GenerateContractNumber(Purchase purchase)
        {
            return $"AC-{DateTime.Now.Year}-{purchase.Id:D6}";
        }

        private string GetDefaultContractTemplate()
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <title>Vehicle Purchase Agreement</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }
        .header { text-align: center; margin-bottom: 30px; }
        .contract-number { text-align: right; margin-bottom: 20px; font-weight: bold; }
        .section { margin-bottom: 25px; }
        .section-title { font-weight: bold; font-size: 16px; margin-bottom: 10px; border-bottom: 1px solid #ccc; padding-bottom: 5px; }
        .info-table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
        .info-table td { padding: 8px; border: 1px solid #ddd; }
        .info-table .label { background-color: #f5f5f5; font-weight: bold; width: 30%; }
        .signature-section { margin-top: 50px; }
        .signature-line { border-bottom: 1px solid #000; width: 300px; margin: 20px 0; }
        .terms { font-size: 12px; margin-top: 30px; }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>VEHICLE PURCHASE AGREEMENT</h1>
        <h2>AutoEdge Dealership</h2>
    </div>
    
    <div class=""contract-number"">
        Contract Number: {{ContractNumber}}
    </div>
    
    <div class=""section"">
        <div class=""section-title"">BUYER INFORMATION</div>
        <table class=""info-table"">
            <tr>
                <td class=""label"">Full Name:</td>
                <td>{{CustomerName}}</td>
            </tr>
            <tr>
                <td class=""label"">Email:</td>
                <td>{{CustomerEmail}}</td>
            </tr>
            <tr>
                <td class=""label"">Phone:</td>
                <td>{{CustomerPhone}}</td>
            </tr>
            <tr>
                <td class=""label"">Address:</td>
                <td>{{CustomerAddress}}</td>
            </tr>
        </table>
    </div>
    
    <div class=""section"">
        <div class=""section-title"">VEHICLE INFORMATION</div>
        <table class=""info-table"">
            <tr>
                <td class=""label"">Make:</td>
                <td>{{VehicleMake}}</td>
            </tr>
            <tr>
                <td class=""label"">Model:</td>
                <td>{{VehicleModel}}</td>
            </tr>
            <tr>
                <td class=""label"">Year:</td>
                <td>{{VehicleYear}}</td>
            </tr>
            <tr>
                <td class=""label"">VIN:</td>
                <td>{{VehicleVIN}}</td>
            </tr>
            <tr>
                <td class=""label"">Mileage:</td>
                <td>{{VehicleMileage}} miles</td>
            </tr>
        </table>
    </div>
    
    <div class=""section"">
        <div class=""section-title"">PURCHASE DETAILS</div>
        <table class=""info-table"">
            <tr>
                <td class=""label"">Purchase Date:</td>
                <td>{{PurchaseDate}}</td>
            </tr>
            <tr>
                <td class=""label"">Purchase Price:</td>
                <td>{{VehiclePrice}}</td>
            </tr>
            <tr>
                <td class=""label"">Total Amount:</td>
                <td>{{TotalAmount}}</td>
            </tr>
        </table>
    </div>
    
    <div class=""section"">
        <div class=""section-title"">TERMS AND CONDITIONS</div>
        <div class=""terms"">
            <p>1. The buyer agrees to purchase the above-described vehicle for the stated purchase price.</p>
            <p>2. The vehicle is sold in 'as-is' condition unless otherwise specified in writing.</p>
            <p>3. The buyer acknowledges receipt of all required documentation and disclosures.</p>
            <p>4. This agreement is binding upon execution by both parties.</p>
            <p>5. Any modifications to this agreement must be made in writing and signed by both parties.</p>
        </div>
    </div>
    
    <div class=""signature-section"">
        <div style=""display: flex; justify-content: space-between;"">
            <div>
                <div class=""signature-line""></div>
                <p>Buyer Signature</p>
                <p>Date: _______________</p>
            </div>
            <div>
                <div class=""signature-line""></div>
                <p>AutoEdge Representative</p>
                <p>Date: {{CurrentDate}}</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}