using AutoEdge.Data;
using AutoEdge.Models.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoEdge.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repository instances
        private IRepository<Vehicle>? _vehicles;
        private IRepository<VehicleImage>? _vehicleImages;
        private IRepository<Customer>? _customers;
        private IRepository<Inquiry>? _inquiries;
        private IRepository<Reservation>? _reservations;
        private IRepository<Document>? _documents;
        private IRepository<Contract>? _contracts;
        private IRepository<Payment>? _payments;
        private IRepository<Purchase>? _purchases;
        private IRepository<Delivery>? _deliveries;
        private IRepository<ServiceBooking>? _serviceBookings;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<Vehicle> Vehicles
        {
            get
            {
                _vehicles ??= new Repository<Vehicle>(_context);
                return _vehicles;
            }
        }

        public IRepository<VehicleImage> VehicleImages
        {
            get
            {
                _vehicleImages ??= new Repository<VehicleImage>(_context);
                return _vehicleImages;
            }
        }

        public IRepository<Customer> Customers
        {
            get
            {
                _customers ??= new Repository<Customer>(_context);
                return _customers;
            }
        }

        public IRepository<Inquiry> Inquiries
        {
            get
            {
                _inquiries ??= new Repository<Inquiry>(_context);
                return _inquiries;
            }
        }

        public IRepository<Reservation> Reservations
        {
            get
            {
                _reservations ??= new Repository<Reservation>(_context);
                return _reservations;
            }
        }

        public IRepository<Document> Documents
        {
            get
            {
                _documents ??= new Repository<Document>(_context);
                return _documents;
            }
        }

        public IRepository<Contract> Contracts
        {
            get
            {
                _contracts ??= new Repository<Contract>(_context);
                return _contracts;
            }
        }

        public IRepository<Payment> Payments
        {
            get
            {
                _payments ??= new Repository<Payment>(_context);
                return _payments;
            }
        }

        public IRepository<Purchase> Purchases
        {
            get
            {
                _purchases ??= new Repository<Purchase>(_context);
                return _purchases;
            }
        }

        public IRepository<Delivery> Deliveries
        {
            get
            {
                _deliveries ??= new Repository<Delivery>(_context);
                return _deliveries;
            }
        }
        public IRepository<ServiceBooking> ServiceBookings
        {
            get
            {
                _serviceBookings ??= new Repository<ServiceBooking>(_context);
                return _serviceBookings;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}