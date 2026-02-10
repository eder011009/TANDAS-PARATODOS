using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AngelesTandas.Domain;
using AngelesTandas.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AngelesTandas.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private string? _currentUserId;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<Tanda> Tandas { get; set; } = null!;
        public DbSet<TandaParticipant> TandaParticipants { get; set; } = null!;
        public DbSet<Turn> Turns { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<PaymentReceipt> PaymentReceipts { get; set; } = null!;
        public DbSet<CommissionRequest> CommissionRequests { get; set; } = null!;
        public DbSet<CommissionResponse> CommissionResponses { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        // Templates
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
        public DbSet<PaymentInstructionTemplate> PaymentInstructionTemplates { get; set; } = null!;

        public void SetCurrentUser(string? userId) => _currentUserId = userId;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Global query filter for ISoftDelete
            foreach (var entityType in builder.Model.GetEntityTypes()
                         .Where(t => typeof(ISoftDelete).IsAssignableFrom(t.ClrType)))
            {
                var method = typeof(ApplicationDbContext).GetMethod(nameof(ApplyIsDeletedFilter),
                    BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { builder });
            }

            // Tanda constraints
            builder.Entity<Tanda>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            });

            // Turn RandomnessToken
            builder.Entity<Turn>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.RandomnessToken).IsRequired().ValueGeneratedNever();
            });

            builder.Entity<AuditLog>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            });

            // Template defaults
            builder.Entity<NotificationTemplate>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Key).IsRequired();
            });

            builder.Entity<PaymentInstructionTemplate>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Key).IsRequired();
            });
        }

        private static void ApplyIsDeletedFilter<TEntity>(ModelBuilder builder) where TEntity : class, ISoftDelete
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override int SaveChanges()
            => SaveChangesAsync(false).GetAwaiter().GetResult();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => SaveChangesAsync(true, cancellationToken);

        private async Task<int> SaveChangesAsync(bool isAsync, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            ChangeTracker.DetectChanges();

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditable || e.Entity is ISoftDelete)
                .ToArray();

            var auditLogs = entries.SelectMany(e =>
            {
                var list = new System.Collections.Generic.List<AuditLog>();
                var entityName = e.Entity.GetType().Name;
                var pk = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? string.Empty;

                if (e.State == EntityState.Added && e.Entity is IAuditable added)
                {
                    added.CreatedAt = now;
                    added.CreatedBy = _currentUserId;
                    list.Add(new AuditLog
                    {
                        ActorUserId = _currentUserId,
                        Action = "Created",
                        Entity = entityName,
                        EntityId = pk,
                        Severity = AuditSeverity.Info,
                        Timestamp = now,
                        CreatedAt = now,
                        CreatedBy = _currentUserId
                    });
                }
                else if (e.State == EntityState.Modified && e.Entity is IAuditable mod)
                {
                    mod.ModifiedAt = now;
                    mod.ModifiedBy = _currentUserId;
                    list.Add(new AuditLog
                    {
                        ActorUserId = _currentUserId,
                        Action = "Modified",
                        Entity = entityName,
                        EntityId = pk,
                        Severity = AuditSeverity.Info,
                        Timestamp = now,
                        CreatedAt = now,
                        CreatedBy = _currentUserId
                    });
                }
                else if (e.State == EntityState.Deleted && e.Entity is ISoftDelete soft)
                {
                    soft.IsDeleted = true;
                    e.State = EntityState.Modified;

                    if (e.Entity is IAuditable aud)
                    {
                        aud.ModifiedAt = now;
                        aud.ModifiedBy = _currentUserId;
                    }

                    list.Add(new AuditLog
                    {
                        ActorUserId = _currentUserId,
                        Action = "SoftDeleted",
                        Entity = entityName,
                        EntityId = pk,
                        Severity = AuditSeverity.Warning,
                        Timestamp = now,
                        CreatedAt = now,
                        CreatedBy = _currentUserId
                    });
                }

                return list;
            }).ToList();

            if (auditLogs.Any())
            {
                AuditLogs.AddRange(auditLogs);
            }

            return isAsync
                ? await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false)
                : base.SaveChanges();
        }
    }
}