using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoEdge.Services
{
    public class ServiceNotificationService : IServiceNotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<ServiceNotificationService> _logger;

        public ServiceNotificationService(ApplicationDbContext db, IEmailService emailService, ILogger<ServiceNotificationService> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendServiceCompletionNotificationAsync(int bookingId)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    _logger.LogWarning("Booking {BookingId} not found for completion notification", bookingId);
                    return false;
                }

                // Generate invoice
                var invoice = await GenerateServiceInvoiceAsync(bookingId);
                
                // Send completion email
                var subject = $"Your {booking.Make} {booking.Model} Service is Complete - Payment Due";
                var body = await GenerateServiceCompletionEmailBodyAsync(booking, invoice);

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);

                // Update booking status
                booking.Status = ServiceBookingStatus.Completed;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Service completion notification sent for booking {BookingId}", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending service completion notification for booking {BookingId}", bookingId);
                return false;
            }
        }

        public async Task<bool> SendPaymentDueNotificationAsync(int bookingId)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return false;
                }

                var invoice = await _db.ServiceInvoices
                    .FirstOrDefaultAsync(i => i.ServiceBookingId == bookingId);

                if (invoice == null)
                {
                    _logger.LogWarning("No invoice found for booking {BookingId}", bookingId);
                    return false;
                }

                var subject = $"Payment Due - Service Invoice #{invoice.InvoiceNumber}";
                var body = await GeneratePaymentDueEmailBodyAsync(booking, invoice);

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);

                _logger.LogInformation("Payment due notification sent for booking {BookingId}", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment due notification for booking {BookingId}", bookingId);
                return false;
            }
        }

        public async Task<bool> SendPaymentConfirmationNotificationAsync(int paymentId)
        {
            try
            {
                var payment = await _db.ServicePayments
                    .Include(p => p.ServiceInvoice)
                    .ThenInclude(i => i.ServiceBooking)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    return false;
                }

                var booking = payment.ServiceInvoice.ServiceBooking;
                var subject = $"Payment Confirmed - Receipt #{payment.TransactionId}";
                var body = await GeneratePaymentConfirmationEmailBodyAsync(booking, payment);

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);

                // Update booking status to paid
                booking.Status = ServiceBookingStatus.Completed;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Payment confirmation notification sent for payment {PaymentId}", paymentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment confirmation notification for payment {PaymentId}", paymentId);
                return false;
            }
        }

        public async Task<bool> SendPaymentReminderNotificationAsync(int bookingId)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return false;
                }

                var invoice = await _db.ServiceInvoices
                    .FirstOrDefaultAsync(i => i.ServiceBookingId == bookingId);

                if (invoice == null)
                {
                    return false;
                }

                // Check if reminder was already sent today
                var lastReminder = await _db.ServicePayments
                    .Where(p => p.ServiceInvoiceId == invoice.Id && p.PaymentMethod == "Reminder")
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefaultAsync();

                if (lastReminder != null && lastReminder.PaymentDate.Date == DateTime.Today)
                {
                    return true; // Already sent today
                }

                var subject = $"Payment Reminder - Service Invoice #{invoice.InvoiceNumber}";
                var body = await GeneratePaymentReminderEmailBodyAsync(booking, invoice);

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);

                // Record reminder
                var reminderPayment = new ServicePayment
                {
                    ServiceInvoiceId = invoice.Id,
                    Amount = 0,
                    PaymentMethod = "Reminder",
                    PaymentDate = DateTime.UtcNow,
                    Status = "ReminderSent",
                    TransactionId = $"REMINDER-{DateTime.UtcNow:yyyyMMddHHmmss}"
                };

                _db.ServicePayments.Add(reminderPayment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Payment reminder sent for booking {BookingId}", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment reminder for booking {BookingId}", bookingId);
                return false;
            }
        }

        public async Task<ServiceInvoice> GenerateServiceInvoiceAsync(int bookingId)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .Include(b => b.ServiceJob)
                    .ThenInclude(j => j.Mechanic)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    throw new InvalidOperationException($"Booking {bookingId} not found");
                }

                // Check if invoice already exists
                var existingInvoice = await _db.ServiceInvoices
                    .FirstOrDefaultAsync(i => i.ServiceBookingId == bookingId);

                if (existingInvoice != null)
                {
                    return existingInvoice;
                }

                // Get ServiceChecklist for this booking - this has the actual pricing!
                var serviceChecklist = await _db.ServiceChecklists
                    .Include(c => c.ServiceJob)
                    .FirstOrDefaultAsync(c => c.ServiceJob.ServiceBookingId == bookingId);

                decimal subtotal;
                decimal partsCost = 0.00m;

                // Use checklist pricing (the source of truth for actual costs)
                if (serviceChecklist != null)
                {
                    // Use actual cost if available, otherwise estimated cost
                    subtotal = serviceChecklist.TotalActualCost > 0 
                        ? serviceChecklist.TotalActualCost 
                        : serviceChecklist.TotalEstimatedCost;
                }
                else
                {
                    // Fallback to service execution details
                    var serviceExecution = await _db.ServiceExecutions
                        .FirstOrDefaultAsync(e => e.ServiceBookingId == bookingId);

                    if (serviceExecution != null && serviceExecution.LaborHours > 0 && serviceExecution.LaborRate > 0)
                    {
                        var laborCost = serviceExecution.LaborHours * serviceExecution.LaborRate;
                        
                        if (!string.IsNullOrEmpty(serviceExecution.PartsUsed))
                        {
                            var parts = JsonSerializer.Deserialize<List<dynamic>>(serviceExecution.PartsUsed);
                            partsCost = parts?.Sum(p => (decimal)p.TotalCost) ?? 0.00m;
                        }
                        
                        subtotal = laborCost + partsCost;
                    }
                    else
                    {
                        // Final fallback: use booking estimate
                        subtotal = booking.EstimatedCost;
                    }
                }

                // Add pickup cost if customer chose pickup service
                decimal pickupCost = 0.00m;
                if (booking.DeliveryMethod == ServiceDeliveryMethod.Pickup)
                {
                    pickupCost = 100.00m; // R100 pickup fee
                    subtotal += pickupCost;
                }

                var tax = subtotal * 0.15m; // 15% tax
                var total = subtotal + tax;

                var invoice = new ServiceInvoice
                {
                    ServiceBookingId = bookingId,
                    InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{bookingId:D4}",
                    IssueDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    SubTotal = subtotal,
                    TaxAmount = tax,
                    TotalAmount = total,
                    Status = "Sent",
                    CreatedAt = DateTime.UtcNow
                };

                _db.ServiceInvoices.Add(invoice);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Generated invoice {InvoiceNumber} for booking {BookingId}", 
                    invoice.InvoiceNumber, bookingId);
                
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for booking {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<bool> SendServiceStatusUpdateAsync(int bookingId, string status, string message)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return false;
                }

                var subject = $"Service Update - {booking.Reference}";
                var body = $@"
                    <h2>Service Status Update</h2>
                    <p>Dear {booking.CustomerName},</p>
                    <p>{message}</p>
                    <p>Service Reference: {booking.Reference}</p>
                    <p>Vehicle: {booking.Make} {booking.Model} ({booking.Year})</p>
                    <p>If you have any questions, please contact us.</p>
                    <p>Best regards,<br>AutoEdge Service Team</p>
                ";

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);

                _logger.LogInformation("Service status update sent for booking {BookingId}: {Status}", bookingId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status update for booking {BookingId}", bookingId);
                return false;
            }
        }

        public async Task<bool> SendDriverAssignmentNotificationAsync(int bookingId, string driverName, string driverPhone)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return false;
                }

                var subject = $"Driver Assigned - Pickup Scheduled";
                var body = $@"
                    <h2>Your Vehicle Pickup is Scheduled</h2>
                    <p>Dear {booking.CustomerName},</p>
                    <p>Your vehicle pickup has been scheduled with the following details:</p>
                    <ul>
                        <li><strong>Driver:</strong> {driverName}</li>
                        <li><strong>Contact:</strong> {driverPhone}</li>
                        <li><strong>Vehicle:</strong> {booking.Make} {booking.Model} ({booking.Year})</li>
                    </ul>
                    <p>The driver will contact you before arrival. Please ensure someone is available to hand over the vehicle.</p>
                    <p>Best regards,<br>AutoEdge Service Team</p>
                ";

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);

                _logger.LogInformation("Driver assignment notification sent for booking {BookingId}", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending driver assignment notification for booking {BookingId}", bookingId);
                return false;
            }
        }

        public async Task<bool> SendPickupScheduledNotificationAsync(int bookingId, DateTime pickupDate, string driverName)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return false;
                }

                var subject = $"Pickup Scheduled - {pickupDate:MMM dd, yyyy}";
                var body = $@"
                    <h2>Vehicle Pickup Scheduled</h2>
                    <p>Dear {booking.CustomerName},</p>
                    <p>Your vehicle pickup has been scheduled for <strong>{pickupDate:MMMM dd, yyyy 'at' h:mm tt}</strong>.</p>
                    <p><strong>Driver:</strong> {driverName}</p>
                    <p><strong>Vehicle:</strong> {booking.Make} {booking.Model} ({booking.Year})</p>
                    <p>Please ensure someone is available at the pickup location. The driver will contact you before arrival.</p>
                    <p>Best regards,<br>AutoEdge Service Team</p>
                ";

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);

                _logger.LogInformation("Pickup scheduled notification sent for booking {BookingId}", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending pickup scheduled notification for booking {BookingId}", bookingId);
                return false;
            }
        }

        public async Task<bool> SendReturnScheduledNotificationAsync(int bookingId, DateTime returnDate, string driverName)
        {
            try
            {
                var booking = await _db.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return false;
                }

                var subject = $"Vehicle Return Scheduled - {returnDate:MMM dd, yyyy}";
                var body = $@"
                    <h2>Vehicle Return Scheduled</h2>
                    <p>Dear {booking.CustomerName},</p>
                    <p>Your vehicle return has been scheduled for <strong>{returnDate:MMMM dd, yyyy 'at' h:mm tt}</strong>.</p>
                    <p><strong>Driver:</strong> {driverName}</p>
                    <p><strong>Vehicle:</strong> {booking.Make} {booking.Model} ({booking.Year})</p>
                    <p>Please ensure someone is available at the return location. The driver will contact you before arrival.</p>
                    <p>Best regards,<br>AutoEdge Service Team</p>
                ";

                await _emailService.SendEmailAsync(booking.CustomerEmail, subject, body);

                _logger.LogInformation("Return scheduled notification sent for booking {BookingId}", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending return scheduled notification for booking {BookingId}", bookingId);
                return false;
            }
        }

        private async Task<string> GenerateServiceCompletionEmailBodyAsync(ServiceBooking booking, ServiceInvoice invoice)
        {
            return $@"
                <h2>Your Vehicle Service is Complete!</h2>
                <p>Dear {booking.CustomerName},</p>
                <p>Great news! The service work on your {booking.Make} {booking.Model} has been completed and your vehicle is ready for pickup.</p>
                
                <h3>Service Summary</h3>
                <ul>
                    <li><strong>Service Type:</strong> {booking.ServiceType}</li>
                    <li><strong>Vehicle:</strong> {booking.Make} {booking.Model} ({booking.Year})</li>
                    <li><strong>Service Reference:</strong> {booking.Reference}</li>
                </ul>

                <h3>Invoice Details</h3>
                <table style='border-collapse: collapse; width: 100%;'>
                    <tr>
                        <td style='border: 1px solid #ddd; padding: 8px;'><strong>Subtotal:</strong></td>
                        <td style='border: 1px solid #ddd; padding: 8px;'>R {invoice.SubTotal:F2}</td>
                    </tr>
                    <tr>
                        <td style='border: 1px solid #ddd; padding: 8px;'><strong>Tax:</strong></td>
                        <td style='border: 1px solid #ddd; padding: 8px;'>R {invoice.TaxAmount:F2}</td>
                    </tr>
                    <tr style='background-color: #f2f2f2;'>
                        <td style='border: 1px solid #ddd; padding: 8px;'><strong>Total Due:</strong></td>
                        <td style='border: 1px solid #ddd; padding: 8px;'><strong>R {invoice.TotalAmount:F2}</strong></td>
                    </tr>
                </table>

                <p><strong>Payment is due within 7 days.</strong></p>
                <p><a href='https://autoedgedealership.azurewebsites.net/ServiceInvoice/Pay/{invoice.Id}' 
                      style='background-color:rgb(2, 2, 2); color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                   Pay Now Online
                </a></p>

                <p>You can also pay in person when you collect your vehicle.</p>
                <p>Best regards,<br>AutoEdge Service Team</p>
            ";
        }

        private async Task<string> GeneratePaymentDueEmailBodyAsync(ServiceBooking booking, ServiceInvoice invoice)
        {
            return $@"
                <h2>Payment Reminder - Service Invoice #{invoice.InvoiceNumber}</h2>
                <p>Dear {booking.CustomerName},</p>
                <p>This is a friendly reminder that payment is due for your recent vehicle service.</p>
                
                <h3>Invoice Details</h3>
                <ul>
                    <li><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</li>
                    <li><strong>Amount Due:</strong> R {invoice.TotalAmount:F2}</li>
                    <li><strong>Due Date:</strong> {invoice.DueDate:MMMM dd, yyyy}</li>
                </ul>

                <p><a href='https://autoedgedealership.azurewebsites.net/ServiceInvoice/Pay/{invoice.Id}' 
                      style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                   Pay Now Online
                </a></p>

                <p>Thank you for choosing AutoEdge for your vehicle service needs.</p>
                <p>Best regards,<br>AutoEdge Service Team</p>
            ";
        }

        private async Task<string> GeneratePaymentConfirmationEmailBodyAsync(ServiceBooking booking, ServicePayment payment)
        {
            return $@"
                <h2>Payment Confirmed - Thank You!</h2>
                <p>Dear {booking.CustomerName},</p>
                <p>Your payment has been successfully processed. Thank you for your business!</p>
                
                <h3>Payment Details</h3>
                <ul>
                    <li><strong>Transaction ID:</strong> {payment.TransactionId}</li>
                    <li><strong>Amount Paid:</strong> ${payment.Amount:F2}</li>
                    <li><strong>Payment Method:</strong> {payment.PaymentMethod}</li>
                    <li><strong>Payment Date:</strong> {payment.PaymentDate:MMMM dd, yyyy 'at' h:mm tt}</li>
                </ul>

                <h3>Next Steps</h3>
                <p>Your vehicle is ready for pickup! You can collect it at your convenience during our business hours:</p>
                <ul>
                    <li><strong>Monday - Friday:</strong> 8:00 AM - 6:00 PM</li>
                    <li><strong>Saturday:</strong> 9:00 AM - 4:00 PM</li>
                </ul>

                <p>Please bring a valid ID when collecting your vehicle.</p>
                <p>Best regards,<br>AutoEdge Service Team</p>
            ";
        }

        private async Task<string> GeneratePaymentReminderEmailBodyAsync(ServiceBooking booking, ServiceInvoice invoice)
        {
            return $@"
                <h2>Payment Reminder - Service Invoice #{invoice.InvoiceNumber}</h2>
                <p>Dear {booking.CustomerName},</p>
                <p>We hope you're satisfied with the service work on your {booking.Make} {booking.Model}.</p>
                <p>This is a friendly reminder that payment is still outstanding for your service invoice.</p>
                
                <h3>Outstanding Amount</h3>
                <p><strong>Total Due:</strong> ${invoice.TotalAmount:F2}</p>
                <p><strong>Original Due Date:</strong> {invoice.DueDate:MMMM dd, yyyy}</p>

                <p><a href='https://autoedgedealership.azurewebsites.net/ServiceInvoice/Pay/{invoice.Id}' 
                      style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                   Pay Now Online
                </a></p>

                <p>If you have any questions about this invoice or need to discuss payment arrangements, please contact us.</p>
                <p>Best regards,<br>AutoEdge Service Team</p>
            ";
        }
    }
}
