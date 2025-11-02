using AutoEdge.Models.Entities;

namespace AutoEdge.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Vehicle> Vehicles { get; }
        IRepository<VehicleImage> VehicleImages { get; }
        IRepository<Customer> Customers { get; }
        IRepository<Inquiry> Inquiries { get; }
        IRepository<Reservation> Reservations { get; }
        IRepository<Document> Documents { get; }
        IRepository<Contract> Contracts { get; }
        IRepository<Payment> Payments { get; }
        IRepository<Purchase> Purchases { get; }
        IRepository<Delivery> Deliveries { get; }
        IRepository<ServiceBooking> ServiceBookings { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}