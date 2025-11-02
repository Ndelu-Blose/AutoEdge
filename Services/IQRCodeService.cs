using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IQRCodeService
    {
        /// <summary>
        /// Generates a QR code for a delivery
        /// </summary>
        /// <param name="deliveryId">The delivery ID</param>
        /// <param name="expiryHours">Hours until QR code expires (default 24)</param>
        /// <returns>QR code data as string</returns>
        Task<string> GenerateQRCodeForDeliveryAsync(int deliveryId, int expiryHours = 24);

        /// <summary>
        /// Verifies a QR code and returns the associated delivery
        /// </summary>
        /// <param name="qrCode">The QR code to verify</param>
        /// <returns>Delivery if valid, null if invalid or expired</returns>
        Task<Delivery?> VerifyQRCodeAsync(string qrCode);

        /// <summary>
        /// Generates QR code image as base64 string
        /// </summary>
        /// <param name="qrCodeData">The QR code data</param>
        /// <returns>Base64 encoded QR code image</returns>
        Task<byte[]> GenerateQRCodeImageAsync(string qrCodeData);

        /// <summary>
        /// Marks a delivery as completed using QR code verification
        /// </summary>
        /// <param name="qrCode">The QR code</param>
        /// <param name="driverUserId">The driver's user ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> CompleteDeliveryWithQRCodeAsync(string qrCode, string driverUserId);




        /// <summary>
        /// Generates a QR code for a service booking
        /// </summary>
        /// <param name="bookingId">The service booking ID</param>
        /// <param name="expiryHours">Hours until QR code expires (default 168 = 7 days)</param>
        /// <returns>QR code data as string</returns>
        Task<string> GenerateQRCodeForServiceBookingAsync(int bookingId, int expiryHours = 168);

        /// <summary>
        /// Verifies a service booking QR code and returns the associated booking
        /// </summary>
        /// <param name="qrCode">The QR code to verify</param>
        /// <returns>ServiceBooking if valid, null if invalid or expired</returns>
        Task<ServiceBooking?> VerifyServiceBookingQRCodeAsync(string qrCode);

        /// <summary>
        /// Marks a service booking as checked in using QR code verification
        /// </summary>
        /// <param name="qrCode">The QR code</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> CheckInServiceBookingWithQRCodeAsync(string qrCode);
    }
}