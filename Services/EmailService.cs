using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace AutoEdge.Services
{
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderPassword = _configuration["Email:SenderPassword"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("Email configuration is incomplete. Please check SMTP settings.");
                    throw new InvalidOperationException("Email configuration is incomplete");
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "AutoEdge"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {email} with subject: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email} with subject: {subject}. Error: {ex.Message}");
                
                // Log specific SMTP errors for debugging
                if (ex is SmtpException smtpEx)
                {
                    _logger.LogError($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
                }
                else if (ex is System.Security.Authentication.AuthenticationException authEx)
                {
                    _logger.LogError($"Authentication Error: {authEx.Message}. Please check email credentials.");
                }
                
                // Don't throw exception to prevent breaking user registration
                // Email failure should not prevent account creation
                _logger.LogWarning($"Email sending failed but continuing with operation for {email}");
            }
        }

        //public async Task SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, byte[] attachmentData, string attachmentName, string attachmentContentType)
        //{
        //    try
        //    {
        //        var smtpServer = _configuration["Email:SmtpServer"];
        //        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        //        var senderEmail = _configuration["Email:SenderEmail"];
        //        var senderPassword = _configuration["Email:SenderPassword"];

        //        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
        //        {
        //            _logger.LogError("Email configuration is incomplete. Please check SMTP settings.");
        //            throw new InvalidOperationException("Email configuration is incomplete");
        //        }

        //        using var client = new SmtpClient(smtpServer, smtpPort)
        //        {
        //            Credentials = new NetworkCredential(senderEmail, senderPassword),
        //            EnableSsl = true
        //        };

        //        var mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(senderEmail, "AutoEdge"),
        //            Subject = subject,
        //            Body = htmlMessage,
        //            IsBodyHtml = true
        //        };

        //        mailMessage.To.Add(email);

        //        // Add attachment
        //        if (attachmentData != null && attachmentData.Length > 0)
        //        {
        //            using var attachmentStream = new MemoryStream(attachmentData);
        //            var attachment = new Attachment(attachmentStream, attachmentName, attachmentContentType);
        //            mailMessage.Attachments.Add(attachment);

        //            await client.SendMailAsync(mailMessage);
        //        }
        //        else
        //        {
        //            await client.SendMailAsync(mailMessage);
        //        }

        //        _logger.LogInformation($"Email with attachment sent successfully to {email} with subject: {subject}");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Failed to send email with attachment to {email} with subject: {subject}. Error: {ex.Message}");

        //        // Log specific SMTP errors for debugging
        //        if (ex is SmtpException smtpEx)
        //        {
        //            _logger.LogError($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
        //        }
        //        else if (ex is System.Security.Authentication.AuthenticationException authEx)
        //         {
        //             _logger.LogError($"Authentication Error: {authEx.Message}. Please check email credentials.");
        //         }

        //        // Don't throw exception to prevent breaking user registration
        //        // Email failure should not prevent account creation
        //        _logger.LogWarning($"Email sending with attachment failed but continuing with operation for {email}");
        //    }
        //}

        public async Task SendEmailWithAttachmentAsync(
    string email,
    string subject,
    string htmlMessage,
    byte[] attachmentData,
    string attachmentName,
    string attachmentContentType)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderPassword = _configuration["Email:SenderPassword"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("Email configuration is incomplete. Please check SMTP settings.");
                    throw new InvalidOperationException("Email configuration is incomplete");
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "AutoEdge"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                if (attachmentData != null && attachmentData.Length > 0)
                {
                    var attachmentStream = new MemoryStream(attachmentData);
                    var attachment = new Attachment(attachmentStream, attachmentName, attachmentContentType);
                    mailMessage.Attachments.Add(attachment);
                }

                await client.SendMailAsync(mailMessage);

                // Cleanup
                foreach (var attachment in mailMessage.Attachments)
                {
                    attachment.Dispose();
                }

                _logger.LogInformation($"Email with attachment sent successfully to {email} with subject: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email with attachment to {email} with subject: {subject}. Error: {ex.Message}");

                if (ex is SmtpException smtpEx)
                {
                    _logger.LogError($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
                }
                else if (ex is System.Security.Authentication.AuthenticationException authEx)
                {
                    _logger.LogError($"Authentication Error: {authEx.Message}. Please check email credentials.");
                }

                _logger.LogWarning($"Email sending with attachment failed but continuing with operation for {email}");
            }
        }

    }

    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendContractSigningEmailAsync(string recipientEmail, string recipientName, string signingUrl, string vehicleInfo);
        Task SendContractSignedNotificationAsync(string recipientEmail, string recipientName, string vehicleInfo);
        Task SendWelcomeEmailAsync(string recipientEmail, string recipientName);
        Task SendPaymentConfirmationEmailAsync(string recipientEmail, string recipientName, string vehicleInfo, decimal amount, string paymentMethod, string transactionId);
        Task SendDeliveryQRCodeEmailAsync(string recipientEmail, string recipientName, string vehicleInfo, string trackingNumber, DateTime scheduledDate, byte[] qrCodeImage);
        Task SendDeliveryQRCodeEmailWithPdfAsync(string recipientEmail, string recipientName, string vehicleInfo, string trackingNumber, DateTime scheduledDate, byte[] pdfAttachment);
                Task SendServiceBookingQRCodeEmailAsync(string recipientEmail, string recipientName, string vehicleInfo, string bookingReference, DateTime scheduledDate, string serviceType, byte[] qrCodeImage);
        Task SendServiceBookingQRCodeEmailWithAttachmentAsync(string recipientEmail, string recipientName, string vehicleInfo, string bookingReference, DateTime scheduledDate, string serviceType, byte[] qrCodeImage);
        Task SendServiceBookingQRCodeEmailWithPdfAsync(string recipientEmail, string recipientName, string vehicleInfo, string bookingReference, DateTime scheduledDate, string serviceType, byte[] pdfAttachment);
    }

    public class CustomEmailService : IEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CustomEmailService> _logger;

        public CustomEmailService(IEmailSender emailSender, ILogger<CustomEmailService> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                await _emailSender.SendEmailAsync(email, subject, htmlMessage);
                _logger.LogInformation("Email sent to {Email} with subject: {Subject}", email, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw;
            }
        }

        public async Task SendContractSigningEmailAsync(string recipientEmail, string recipientName, string signingUrl, string vehicleInfo)
        {
            var subject = "Contract Ready for Electronic Signature - AutoEdge";
            var htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>AutoEdge</h1>
                            <h2>Contract Ready for Signature</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {recipientName},</p>
                            <p>Your vehicle purchase contract is ready for electronic signature.</p>
                            <p><strong>Vehicle Details:</strong> {vehicleInfo}</p>
                            <p>Please review and sign your contract by clicking the button below:</p>
                            <p style='text-align: center;'>
                                <a href='{signingUrl}' class='button'>Sign Contract Now</a>
                            </p>
                            <p>If you have any questions about your purchase or the signing process, please don't hesitate to contact us.</p>
                            <p>Thank you for choosing AutoEdge!</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                            <p>If you need assistance, please contact our support team.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
            _logger.LogInformation($"Contract signing email sent to {recipientEmail} for vehicle: {vehicleInfo}");
        }

        public async Task SendContractSignedNotificationAsync(string recipientEmail, string recipientName, string vehicleInfo)
        {
            var subject = "Contract Successfully Signed - AutoEdge";
            var htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>AutoEdge</h1>
                            <h2>Contract Successfully Signed!</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {recipientName},</p>
                            <p>Congratulations! Your vehicle purchase contract has been successfully signed.</p>
                            <p><strong>Vehicle Details:</strong> {vehicleInfo}</p>
                            <p>Your purchase is now being processed and we will contact you soon with next steps for delivery.</p>
                            <p>Thank you for choosing AutoEdge!</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                            <p>If you need assistance, please contact our support team.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
            _logger.LogInformation($"Contract signed notification sent to {recipientEmail} for vehicle: {vehicleInfo}");
        }

        public async Task SendWelcomeEmailAsync(string recipientEmail, string recipientName)
        {
            var subject = "Welcome to AutoEdge!";
            var htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to AutoEdge!</h1>
                        </div>
                        <div class='content'>
                            <p>Dear {recipientName},</p>
                            <p>Welcome to AutoEdge! We're excited to help you find your perfect vehicle.</p>
                            <p>You can now browse our inventory, schedule test drives, and manage your purchases through your account.</p>
                            <p>If you have any questions, our team is here to help!</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                            <p>If you need assistance, please contact our support team.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
            _logger.LogInformation($"Welcome email sent to {recipientEmail}");
        }

        public async Task SendPaymentConfirmationEmailAsync(string recipientEmail, string recipientName, string vehicleInfo, decimal amount, string paymentMethod, string transactionId)
        {
            var subject = "Payment Confirmation - AutoEdge";
            var htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .payment-details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745; }}
                        .amount {{ font-size: 24px; font-weight: bold; color: #28a745; text-align: center; margin: 20px 0; }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                        .receipt-info {{ display: flex; justify-content: space-between; margin: 10px 0; }}
                        .receipt-info strong {{ color: #333; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>AutoEdge</h1>
                            <h2>Payment Confirmation</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {recipientName},</p>
                            <p>Thank you for your payment! We have successfully received your payment for your vehicle purchase.</p>
                            
                            <div class='amount'>
                                ${amount:F2} USD
                            </div>
                            
                            <div class='payment-details'>
                                <h3 style='margin-top: 0; color: #28a745;'>Payment Details</h3>
                                <div class='receipt-info'>
                                    <span><strong>Vehicle:</strong></span>
                                    <span>{vehicleInfo}</span>
                                </div>
                                <div class='receipt-info'>
                                    <span><strong>Payment Method:</strong></span>
                                    <span>{paymentMethod}</span>
                                </div>
                                <div class='receipt-info'>
                                    <span><strong>Transaction ID:</strong></span>
                                    <span>{transactionId}</span>
                                </div>
                                <div class='receipt-info'>
                                    <span><strong>Payment Date:</strong></span>
                                    <span>{DateTime.Now:MMM dd, yyyy HH:mm}</span>
                                </div>
                                <div class='receipt-info'>
                                    <span><strong>Status:</strong></span>
                                    <span style='color: #28a745; font-weight: bold;'>PAID</span>
                                </div>
                            </div>
                            
                            <p>This serves as your proof of payment. Please keep this email for your records.</p>
                            <p>Your vehicle purchase is now being processed and we will contact you soon with next steps for delivery.</p>
                            <p>If you have any questions about your payment or purchase, please don't hesitate to contact us.</p>
                            <p>Thank you for choosing AutoEdge!</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                            <p>If you need assistance, please contact our support team.</p>
                            <p>AutoEdge Dealership | 123 Auto Street, City, State 12345 | (555) 123-4567</p>
                        </div>
                    </div>
                </body>
                </html>";

            await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
            _logger.LogInformation($"Payment confirmation email sent to {recipientEmail} for transaction {transactionId}");
        }

        public async Task SendDeliveryQRCodeEmailAsync(string recipientEmail, string recipientName, string vehicleInfo, string trackingNumber, DateTime scheduledDate, byte[] qrCodeImage)
        {
            var subject = $"Delivery QR Code - {vehicleInfo} (Tracking: {trackingNumber})";
            
            var qrCodeBase64 = Convert.ToBase64String(qrCodeImage);
            var qrCodeDataUri = $"data:image/png;base64,{qrCodeBase64}";
            
            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        .container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 30px; background-color: #f8f9fa; }}
                        .qr-section {{ text-align: center; margin: 30px 0; padding: 20px; background-color: white; border-radius: 8px; border: 2px dashed #007bff; }}
                        .qr-code {{ max-width: 200px; height: auto; margin: 20px 0; }}
                        .delivery-info {{ background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .info-row {{ display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }}
                        .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; font-size: 12px; }}
                        .important {{ color: #dc3545; font-weight: bold; }}
                        .success {{ color: #28a745; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🚚 Delivery QR Code</h1>
                            <p>Your vehicle delivery verification code</p>
                        </div>
                        <div class='content'>
                            <h2>Hello {recipientName},</h2>
                            <p>Your vehicle delivery has been scheduled! Please find your delivery QR code below.</p>
                            
                            <div class='delivery-info'>
                                <h3>📋 Delivery Details</h3>
                                <div class='info-row'>
                                    <span><strong>Vehicle:</strong></span>
                                    <span>{vehicleInfo}</span>
                                </div>
                                <div class='info-row'>
                                    <span><strong>Tracking Number:</strong></span>
                                    <span class='success'>{trackingNumber}</span>
                                </div>
                                <div class='info-row'>
                                    <span><strong>Scheduled Date:</strong></span>
                                    <span>{scheduledDate:MMM dd, yyyy HH:mm}</span>
                                </div>
                            </div>
                            
                            <div class='qr-section'>
                                <h3>📱 Your Delivery QR Code</h3>
                                <p>Present this QR code to the delivery driver for verification:</p>
                                <img src='{qrCodeDataUri}' alt='Delivery QR Code' class='qr-code' />
                                <p class='important'>⚠️ Keep this QR code secure and only share it with the authorized delivery driver.</p>
                                <p><strong>QR Code expires in 30 days from delivery scheduling.</strong></p>
                            </div>
                            
                            <div style='background-color: #e7f3ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                                <h4>📋 Delivery Instructions:</h4>
                                <ul>
                                    <li>Be available at the scheduled delivery time</li>
                                    <li>Have this QR code ready on your phone or printed</li>
                                    <li>Verify the driver's identity before showing the QR code</li>
                                    <li>Inspect the vehicle before accepting delivery</li>
                                    <li>The driver will scan this QR code to complete the delivery</li>
                                </ul>
                            </div>
                            
                            <p>If you need to reschedule or have any questions about your delivery, please contact our delivery team.</p>
                            <p>Thank you for choosing AutoEdge!</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                            <p>If you need assistance, please contact our delivery team.</p>
                            <p>AutoEdge Dealership | 123 Auto Street, City, State 12345 | (555) 123-4567</p>
                        </div>
                    </div>
                </body>
                </html>";

            await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
            _logger.LogInformation($"Delivery QR code email sent to {recipientEmail} for tracking {trackingNumber}");
        }

        public async Task SendDeliveryQRCodeEmailWithPdfAsync(string recipientEmail, string recipientName, string vehicleInfo, string trackingNumber, DateTime scheduledDate, byte[] pdfAttachment)
        {
            var subject = $"Delivery QR Code - {vehicleInfo} (Tracking: {trackingNumber})";
            
            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        .container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 30px; background-color: #f8f9fa; }}
                        .delivery-info {{ background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .info-row {{ display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }}
                        .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; font-size: 12px; }}
                        .important {{ color: #dc3545; font-weight: bold; }}
                        .success {{ color: #28a745; font-weight: bold; }}
                        .pdf-notice {{ background-color: #e7f3ff; padding: 15px; border-radius: 8px; margin: 20px 0; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🚚 Delivery QR Code</h1>
                            <p>Your vehicle delivery verification code</p>
                        </div>
                        <div class='content'>
                            <h2>Hello {recipientName},</h2>
                            <p>Your vehicle delivery has been scheduled! Please find your delivery QR code attached as a PDF document.</p>
                            
                            <div class='delivery-info'>
                                <h3>📋 Delivery Details</h3>
                                <div class='info-row'>
                                    <span><strong>Vehicle:</strong></span>
                                    <span>{vehicleInfo}</span>
                                </div>
                                <div class='info-row'>
                                    <span><strong>Tracking Number:</strong></span>
                                    <span class='success'>{trackingNumber}</span>
                                </div>
                                <div class='info-row'>
                                    <span><strong>Scheduled Date:</strong></span>
                                    <span>{scheduledDate:MMM dd, yyyy HH:mm}</span>
                                </div>
                            </div>
                            
                            <div class='pdf-notice'>
                                <h3>📄 QR Code PDF Attachment</h3>
                                <p>Your delivery QR code is attached as a PDF document to this email.</p>
                                <p class='important'>⚠️ Please download and save the PDF. Present the QR code to the delivery driver for verification.</p>
                                <p><strong>QR Code expires in 30 days from delivery scheduling.</strong></p>
                            </div>
                            
                            <div style='background-color: #e7f3ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                                <h4>📋 Delivery Instructions:</h4>
                                <ul>
                                    <li>Download and save the attached PDF with your QR code</li>
                                    <li>Be available at the scheduled delivery time</li>
                                    <li>Have the QR code ready on your phone or printed from the PDF</li>
                                    <li>Verify the driver's identity before showing the QR code</li>
                                    <li>Inspect the vehicle before accepting delivery</li>
                                    <li>The driver will scan this QR code to complete the delivery</li>
                                </ul>
                            </div>
                            
                            <p>If you need to reschedule or have any questions about your delivery, please contact our delivery team.</p>
                            <p>Thank you for choosing AutoEdge!</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                            <p>If you need assistance, please contact our delivery team.</p>
                            <p>AutoEdge Dealership | 123 Auto Street, City, State 12345 | (555) 123-4567</p>
                        </div>
                    </div>
                </body>
                </html>";

            // Use the EmailService's SendEmailWithAttachmentAsync method
            if (_emailSender is EmailService emailService)
            {
                await emailService.SendEmailWithAttachmentAsync(
                    recipientEmail, 
                    subject, 
                    htmlBody, 
                    pdfAttachment, 
                    $"DeliveryQRCode_{trackingNumber}.pdf", 
                    "application/pdf"
                );
            }
            else
            {
                // Fallback to regular email if attachment method is not available
                await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
            }
            
            _logger.LogInformation($"Delivery QR code email with PDF attachment sent to {recipientEmail} for tracking {trackingNumber}");
        }
    
        public async Task SendServiceBookingQRCodeEmailAsync(string recipientEmail, string recipientName, string vehicleInfo, string bookingReference, DateTime scheduledDate, string serviceType, byte[] qrCodeImage)
    {
        var subject = $"Service Booking QR Code - {vehicleInfo} (Ref: {bookingReference})";
        
        var qrCodeBase64 = Convert.ToBase64String(qrCodeImage);
        var qrCodeDataUri = $"data:image/png;base64,{qrCodeBase64}";
        
        var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
                    .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 30px; background-color: #f8f9fa; }}
                    .qr-section {{ text-align: center; margin: 30px 0; padding: 20px; background-color: white; border-radius: 8px; border: 2px dashed #28a745; }}
                    .qr-code {{ max-width: 250px; height: auto; margin: 20px 0; border: 1px solid #ddd; border-radius: 8px; }}
                    .booking-info {{ background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    .info-row {{ display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }}
                    .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; font-size: 12px; }}
                    .important {{ color: #dc3545; font-weight: bold; }}
                    .success {{ color: #28a745; font-weight: bold; }}
                    .service-badge {{ background-color: #28a745; color: white; padding: 4px 8px; border-radius: 4px; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔧 Service Booking QR Code</h1>
                        <p>Your vehicle service check-in code</p>
                    </div>
                    <div class='content'>
                        <h2>Hello {recipientName},</h2>
                        <p>Your vehicle service has been scheduled! Please find your service QR code below.</p>
                        
                        <div class='booking-info'>
                            <h3>📋 Service Details</h3>
                            <div class='info-row'>
                                <span><strong>Vehicle:</strong></span>
                                <span>{vehicleInfo}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Booking Reference:</strong></span>
                                <span class='success'>{bookingReference}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Service Type:</strong></span>
                                <span class='service-badge'>{serviceType}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Scheduled Date:</strong></span>
                                <span>{scheduledDate:MMM dd, yyyy HH:mm}</span>
                            </div>
                        </div>
                        
                        <div class='qr-section'>
                            <h3>📱 Your Service QR Code</h3>
                            <p>Present this QR code when you arrive for your service appointment:</p>
                            <div style='text-align: center; margin: 20px 0;'>
                                <img src='{qrCodeDataUri}' alt='Service QR Code' style='display: block; margin: 0 auto; max-width: 250px; height: auto; border: 1px solid #ddd; border-radius: 8px; padding: 10px; background: white;' />
                            </div>
                            <p class='important'>⚠️ Keep this QR code secure and only share it at the service center.</p>
                            <p><strong>QR Code expires in 7 days from booking date.</strong></p>
                        </div>
                        
                        <div style='background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h4>📋 Service Check-in Instructions:</h4>
                            <ul>
                                <li>Arrive at the service center at your scheduled time</li>
                                <li>Have this QR code ready on your phone or printed</li>
                                <li>Present the QR code to the service receptionist</li>
                                <li>The QR code will be scanned to confirm your booking and vehicle details</li>
                                <li>Once scanned, your booking will be marked as checked in</li>
                                <li>You'll receive updates on your service progress</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <h4>⚠️ Important Reminders:</h4>
                            <ul>
                                <li>Bring your vehicle registration and insurance documents</li>
                                <li>Remove all personal belongings from your vehicle</li>
                                <li>Inform us of any specific issues or concerns</li>
                                <li>Estimated service time: 1-3 hours depending on service type</li>
                            </ul>
                        </div>
                        
                        <p>If you need to reschedule or have any questions about your service appointment, please contact our service team.</p>
                        <p>Thank you for choosing AutoEdge for your vehicle service needs!</p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                        <p>If you need assistance, please contact our service team.</p>
                        <p>AutoEdge Service Center | 123 Auto Street, City, State 12345 | (555) 123-4567</p>
                    </div>
                </div>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
        _logger.LogInformation($"Service booking QR code email sent to {recipientEmail} for booking {bookingReference}");
    }

    public async Task SendServiceBookingQRCodeEmailWithAttachmentAsync(string recipientEmail, string recipientName, string vehicleInfo, string bookingReference, DateTime scheduledDate, string serviceType, byte[] qrCodeImage)
    {
        var subject = $"Service Booking QR Code - {vehicleInfo} (Ref: {bookingReference})";
        
        var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
                    .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 30px; background-color: #f8f9fa; }}
                    .qr-section {{ text-align: center; margin: 30px 0; padding: 20px; background-color: white; border-radius: 8px; border: 2px dashed #28a745; }}
                    .booking-info {{ background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    .info-row {{ display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }}
                    .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; font-size: 12px; }}
                    .important {{ color: #dc3545; font-weight: bold; }}
                    .success {{ color: #28a745; font-weight: bold; }}
                    .service-badge {{ background-color: #28a745; color: white; padding: 4px 8px; border-radius: 4px; font-size: 12px; }}
                    .attachment-notice {{ background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin: 20px 0; text-align: center; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔧 Service Booking QR Code</h1>
                        <p>Your vehicle service check-in code</p>
                    </div>
                    <div class='content'>
                        <h2>Hello {recipientName},</h2>
                        <p>Your vehicle service has been scheduled! Please find your service QR code attached to this email.</p>
                        
                        <div class='booking-info'>
                            <h3>📋 Service Details</h3>
                            <div class='info-row'>
                                <span><strong>Vehicle:</strong></span>
                                <span>{vehicleInfo}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Booking Reference:</strong></span>
                                <span class='success'>{bookingReference}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Service Type:</strong></span>
                                <span class='service-badge'>{serviceType}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Scheduled Date:</strong></span>
                                <span>{scheduledDate:MMM dd, yyyy HH:mm}</span>
                            </div>
                        </div>
                        
                        <div class='attachment-notice'>
                            <h3>📱 Your Service QR Code</h3>
                            <p><strong>Your QR code is attached as an image file to this email.</strong></p>
                            <p>Please download and save the QR code image, then present it when you arrive for your service appointment.</p>
                            <p class='important'>⚠️ Keep this QR code secure and only share it at the service center.</p>
                            <p><strong>QR Code expires in 7 days from booking date.</strong></p>
                        </div>
                        
                        <div style='background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h4>📋 Service Check-in Instructions:</h4>
                            <ul>
                                <li>Download the attached QR code image</li>
                                <li>Arrive at the service center at your scheduled time</li>
                                <li>Have the QR code ready on your phone or printed</li>
                                <li>Present the QR code to the service receptionist</li>
                                <li>The QR code will be scanned to confirm your booking and vehicle details</li>
                                <li>Once scanned, your booking will be marked as checked in</li>
                                <li>You'll receive updates on your service progress</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <h4>⚠️ Important Reminders:</h4>
                            <ul>
                                <li>Bring your vehicle registration and insurance documents</li>
                                <li>Remove all personal belongings from your vehicle</li>
                                <li>Inform us of any specific issues or concerns</li>
                                <li>Estimated service time: 1-3 hours depending on service type</li>
                            </ul>
                        </div>
                        
                        <p>If you need to reschedule or have any questions about your service appointment, please contact our service team.</p>
                        <p>Thank you for choosing AutoEdge for your vehicle service needs!</p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                        <p>If you need assistance, please contact our service team.</p>
                        <p>AutoEdge Service Center | 123 Auto Street, City, State 12345 | (555) 123-4567</p>
                    </div>
                </div>
            </body>
            </html>";

        // Use the EmailService's SendEmailWithAttachmentAsync method
        if (_emailSender is EmailService emailService)
        {
            await emailService.SendEmailWithAttachmentAsync(
                recipientEmail, 
                subject, 
                htmlBody, 
                qrCodeImage, 
                $"ServiceQRCode_{bookingReference}.png", 
                "image/png"
            );
        }
        else
        {
            // Fallback to regular email if attachment method is not available
            await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
        }
        
        _logger.LogInformation($"Service booking QR code email with attachment sent to {recipientEmail} for booking {bookingReference}");
    }

    public async Task SendServiceBookingQRCodeEmailWithPdfAsync(string recipientEmail, string recipientName, string vehicleInfo, string bookingReference, DateTime scheduledDate, string serviceType, byte[] pdfAttachment)
    {
        var subject = $"Service Booking QR Code Ticket - {vehicleInfo} (Ref: {bookingReference})";
        
        var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
                    .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 30px; background-color: #f8f9fa; }}
                    .ticket-notice {{ text-align: center; margin: 30px 0; padding: 20px; background-color: white; border-radius: 8px; border: 2px solid #28a745; }}
                    .booking-info {{ background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    .info-row {{ display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }}
                    .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; font-size: 12px; }}
                    .important {{ color: #dc3545; font-weight: bold; }}
                    .success {{ color: #28a745; font-weight: bold; }}
                    .service-badge {{ background-color: #28a745; color: white; padding: 4px 8px; border-radius: 4px; font-size: 12px; }}
                    .pdf-notice {{ background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin: 20px 0; text-align: center; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🎫 Service Booking QR Code Ticket</h1>
                        <p>Your vehicle service check-in ticket</p>
                    </div>
                    <div class='content'>
                        <h2>Hello {recipientName},</h2>
                        <p>Your vehicle service has been scheduled! Please find your service QR code ticket attached as a PDF document.</p>
                        
                        <div class='booking-info'>
                            <h3>📋 Service Details</h3>
                            <div class='info-row'>
                                <span><strong>Vehicle:</strong></span>
                                <span>{vehicleInfo}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Booking Reference:</strong></span>
                                <span class='success'>{bookingReference}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Service Type:</strong></span>
                                <span class='service-badge'>{serviceType}</span>
                            </div>
                            <div class='info-row'>
                                <span><strong>Scheduled Date:</strong></span>
                                <span>{scheduledDate:MMM dd, yyyy HH:mm}</span>
                            </div>
                        </div>
                        
                        <div class='pdf-notice'>
                            <h3>🎫 Your QR Code Ticket</h3>
                            <p><strong>Your QR code ticket is attached as a PDF document to this email.</strong></p>
                            <p>Please download and print the PDF ticket, or save it on your phone for easy access.</p>
                            <p class='important'>⚠️ Bring this ticket to the service center for check-in.</p>
                            <p><strong>QR Code expires in 7 days from booking date.</strong></p>
                        </div>
                        
                        <div style='background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h4>📋 Service Check-in Instructions:</h4>
                            <ul>
                                <li>Download and save the attached PDF ticket</li>
                                <li>Print the ticket or save it on your phone</li>
                                <li>Arrive at the service center at your scheduled time</li>
                                <li>Present the QR code ticket to the service receptionist</li>
                                <li>The QR code will be scanned to confirm your booking and vehicle details</li>
                                <li>Once scanned, your booking will be marked as checked in</li>
                                <li>You'll receive updates on your service progress</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <h4>⚠️ Important Reminders:</h4>
                            <ul>
                                <li>Bring your vehicle registration and insurance documents</li>
                                <li>Remove all personal belongings from your vehicle</li>
                                <li>Inform us of any specific issues or concerns</li>
                                <li>Estimated service time: 1-3 hours depending on service type</li>
                            </ul>
                        </div>
                        
                        <p>If you need to reschedule or have any questions about your service appointment, please contact our service team.</p>
                        <p>Thank you for choosing AutoEdge for your vehicle service needs!</p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message from AutoEdge. Please do not reply to this email.</p>
                        <p>If you need assistance, please contact our service team.</p>
                        <p>AutoEdge Service Center | 123 Auto Street, City, State 12345 | (555) 123-4567</p>
                    </div>
                </div>
            </body>
            </html>";

        // Use the EmailService's SendEmailWithAttachmentAsync method
        if (_emailSender is EmailService emailService)
        {
            await emailService.SendEmailWithAttachmentAsync(
                recipientEmail, 
                subject, 
                htmlBody, 
                pdfAttachment, 
                $"ServiceBookingTicket_{bookingReference}.pdf", 
                "application/pdf"
            );
        }
        else
        {
            // Fallback to regular email if attachment method is not available
            await _emailSender.SendEmailAsync(recipientEmail, subject, htmlBody);
        }
        
        _logger.LogInformation($"Service booking QR code email with PDF ticket sent to {recipientEmail} for booking {bookingReference}");
    }
}
}