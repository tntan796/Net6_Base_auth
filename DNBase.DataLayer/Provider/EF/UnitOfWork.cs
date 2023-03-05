using DNBase.Common;
using DNBase.DataLayer.EF.Entities;
using DNBase.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.DataLayer.EF
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(bool ensureAutoHistory = false);
    }

    public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly ICurrentPrincipal _currentPrincipal;
        private bool disposed = false;
        protected readonly ILogger<IUnitOfWork> _logger;
        private PrincipalModel _principal = null;

        public UnitOfWork(TContext context 
                         ,ILogger<IUnitOfWork> logger
                         , ICurrentPrincipal currentPrincipal)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentPrincipal = currentPrincipal;
            _logger = logger;
            _principal = _currentPrincipal.Principal;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            disposed = true;
        }

        public async Task<int> SaveChangesAsync(bool ensureAutoHistory = false)
        {
            if (ensureAutoHistory)
            {
                _context.EnsureAutoHistory();
            }
            var currentDetectChangesSetting = _context.ChangeTracker.AutoDetectChangesEnabled;

            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                _context.ChangeTracker.DetectChanges();

                var modifiedEntries = _context.ChangeTracker.Entries<IAuditedEntity>()
                    .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted);

                AuditEntity(modifiedEntries);

                _context.ChangeTracker.DetectChanges();

                return await _context.SaveChangesAsync();
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = currentDetectChangesSetting;
            }
        }

        private void AuditEntity(IEnumerable<EntityEntry<IAuditedEntity>> modifiedEntries)
        {
            var now = DateTime.Now;
            var userId = _principal != null ? _principal.UserId : Guid.Empty;
            foreach (var entry in modifiedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = userId;
                        AuditUpdatingField(entry);

                        _logger.LogInformation("Added entity {@Entity} by user {UserId}", entry.Entity, userId);
                        break;

                    case EntityState.Modified:
                        AuditUpdatingField(entry);

                        _logger.LogInformation("Updated entity to {@Entity} by user {UserId}", entry.Entity, userId);
                        break;

                    case EntityState.Deleted:
                        _logger.LogInformation("Deleted entity {@Entity} by user {UserId}", entry.Entity, userId);
                        break;
                }
            }

            void AuditUpdatingField(EntityEntry<IAuditedEntity> entry)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }
}