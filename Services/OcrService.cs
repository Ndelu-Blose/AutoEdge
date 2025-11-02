using AutoEdge.Models;
using AutoEdge.Models.Entities;
using System.Text.RegularExpressions;

namespace AutoEdge.Services
{
    public class OcrService : IOcrService
    {
        private readonly ILogger<OcrService> _logger;
        private readonly Random _random = new();

        public OcrService(ILogger<OcrService> logger)
        {
            _logger = logger;
        }

        public async Task<OcrResult> ExtractTextAsync(string filePath, DocumentType documentType)
        {
            try
            {
                // Simulate OCR processing delay
                await Task.Delay(2000);

                // Simulate OCR text extraction based on document type
                var extractedText = SimulateTextExtraction(documentType);
                var extractedFields = ExtractFieldsFromText(extractedText, documentType);

                return new OcrResult
                {
                    Success = true,
                    ExtractedText = extractedText,
                    ExtractedFields = extractedFields
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OCR processing for file: {FilePath}", filePath);
                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = "OCR processing failed: " + ex.Message
                };
            }
        }

        public async Task<ValidationResult> ValidateDocumentAsync(string extractedText, DocumentType documentType)
        {
            await Task.Delay(1000); // Simulate validation processing

            var result = new ValidationResult
            {
                ConfidenceScore = _random.NextDouble() * 0.3 + 0.7 // 70-100% confidence
            };

            switch (documentType.Name.ToLower())
            {
                case "driver's license":
                    result = ValidateDriversLicense(extractedText);
                    break;
                case "national id":
                case "passport":
                    result = ValidateNationalId(extractedText);
                    break;
                case "proof of income":
                    result = ValidateProofOfIncome(extractedText);
                    break;
                case "insurance certificate":
                    result = ValidateInsurance(extractedText);
                    break;
                default:
                    result = ValidateGenericDocument(extractedText);
                    break;
            }

            result.ConfidenceScore = _random.NextDouble() * 0.3 + 0.7;
            return result;
        }

        private string SimulateTextExtraction(DocumentType documentType)
        {
            return documentType.Name.ToLower() switch
            {
                "driver's license" => GenerateDriversLicenseText(),
                "national id" => GenerateNationalIdText(),
                "passport" => GeneratePassportText(),
                "proof of income" => GenerateProofOfIncomeText(),
                "insurance certificate" => GenerateInsuranceText(),
                _ => "Sample extracted text from document"
            };
        }

        private Dictionary<string, string> ExtractFieldsFromText(string text, DocumentType documentType)
        {
            var fields = new Dictionary<string, string>();

            switch (documentType.Name.ToLower())
            {
                case "driver's license":
                    fields["LicenseNumber"] = ExtractPattern(text, @"License No[.:]?\s*([A-Z0-9]+)");
                    fields["FullName"] = ExtractPattern(text, @"Name[.:]?\s*([A-Za-z\s]+)");
                    fields["DateOfBirth"] = ExtractPattern(text, @"DOB[.:]?\s*(\d{2}/\d{2}/\d{4})");
                    fields["ExpiryDate"] = ExtractPattern(text, @"Expires[.:]?\s*(\d{2}/\d{2}/\d{4})");
                    break;
                case "national id":
                    fields["IdNumber"] = ExtractPattern(text, @"ID No[.:]?\s*([0-9]+)");
                    fields["FullName"] = ExtractPattern(text, @"Name[.:]?\s*([A-Za-z\s]+)");
                    fields["DateOfBirth"] = ExtractPattern(text, @"DOB[.:]?\s*(\d{2}/\d{2}/\d{4})");
                    break;
                case "proof of income":
                    fields["EmployerName"] = ExtractPattern(text, @"Employer[.:]?\s*([A-Za-z\s&.,]+)");
                    fields["AnnualSalary"] = ExtractPattern(text, @"Annual Salary[.:]?\s*\$([0-9,]+)");
                    fields["EmployeeName"] = ExtractPattern(text, @"Employee[.:]?\s*([A-Za-z\s]+)");
                    break;
            }

            return fields;
        }

        private string ExtractPattern(string text, string pattern)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        private ValidationResult ValidateDriversLicense(string text)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (!text.Contains("License No", StringComparison.OrdinalIgnoreCase))
            {
                result.ValidationErrors.Add("License number not found");
                result.IsValid = false;
            }
            
            if (!Regex.IsMatch(text, @"\d{2}/\d{2}/\d{4}"))
            {
                result.ValidationErrors.Add("Valid date format not found");
                result.IsValid = false;
            }

            return result;
        }

        private ValidationResult ValidateNationalId(string text)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (!text.Contains("ID No", StringComparison.OrdinalIgnoreCase))
            {
                result.ValidationErrors.Add("ID number not found");
                result.IsValid = false;
            }

            return result;
        }

        private ValidationResult ValidateProofOfIncome(string text)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (!text.Contains("Salary", StringComparison.OrdinalIgnoreCase) && 
                !text.Contains("Income", StringComparison.OrdinalIgnoreCase))
            {
                result.ValidationErrors.Add("Income information not found");
                result.IsValid = false;
            }

            return result;
        }

        private ValidationResult ValidateInsurance(string text)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (!text.Contains("Insurance", StringComparison.OrdinalIgnoreCase))
            {
                result.ValidationErrors.Add("Insurance information not found");
                result.IsValid = false;
            }

            return result;
        }

        private ValidationResult ValidateGenericDocument(string text)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (string.IsNullOrWhiteSpace(text) || text.Length < 10)
            {
                result.ValidationErrors.Add("Insufficient text content");
                result.IsValid = false;
            }

            return result;
        }

        private string GenerateDriversLicenseText()
        {
            return $@"DRIVER LICENSE
License No: DL{_random.Next(100000, 999999)}
Name: John Smith
Address: 123 Main St, City, State 12345
DOB: 01/15/1990
Sex: M
Height: 5'10""
Weight: 180
Eyes: BRN
Class: C
Restrictions: NONE
Endorsements: NONE
Issued: 03/15/2020
Expires: 03/15/2025";
        }

        private string GenerateNationalIdText()
        {
            return $@"NATIONAL IDENTITY CARD
ID No: {_random.Next(100000000, 999999999)}
Name: John Smith
Nationality: American
DOB: 01/15/1990
Sex: Male
Place of Birth: New York, NY
Issued: 01/01/2020
Expires: 01/01/2030";
        }

        private string GeneratePassportText()
        {
            return $@"PASSPORT
Passport No: P{_random.Next(1000000, 9999999)}
Type: P
Country Code: USA
Surname: SMITH
Given Names: JOHN MICHAEL
Nationality: USA
Date of Birth: 15 JAN 1990
Sex: M
Place of Birth: NEW YORK, NY, USA
Date of Issue: 15 MAR 2020
Date of Expiry: 15 MAR 2030";
        }

        private string GenerateProofOfIncomeText()
        {
            return $@"EMPLOYMENT VERIFICATION LETTER
Employer: ABC Corporation
Employee: John Smith
Position: Software Engineer
Employment Start Date: 01/15/2020
Employment Status: Full-time
Annual Salary: ${_random.Next(50000, 150000):N0}
Hourly Rate: ${_random.Next(25, 75)}.00
Average Hours per Week: 40
This letter confirms the above employment details.
HR Department
ABC Corporation";
        }

        private string GenerateInsuranceText()
        {
            return $@"CERTIFICATE OF INSURANCE
Insurance Company: XYZ Insurance Co.
Policy Number: INS{_random.Next(100000, 999999)}
Policyholder: John Smith
Coverage Type: Auto Insurance
Policy Period: 01/01/2024 to 01/01/2025
Vehicle: 2020 Honda Accord
VIN: 1HGCV1F3{_random.Next(10000000, 99999999)}
Coverage Limits:
Bodily Injury: $100,000/$300,000
Property Damage: $50,000
Comprehensive: $1,000 deductible
Collision: $1,000 deductible";
        }
    }
}