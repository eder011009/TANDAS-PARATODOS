using System;
using System.Threading.Tasks;
using AngelesTandas.Application;
using AngelesTandas.Infrastructure.Data;
using AngelesTandas.Domain;

namespace AngelesTandas.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;

        public AuditService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogActionAsync(string actorUserId, string action, string entity, string entityId, AuditSeverity severity = AuditSeverity.Info)
        {
            var log = new AuditLog
            {
                ActorUserId = actorUserId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Severity = severity,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actorUserId
            };

            _db.SetCurrentUser(actorUserId);
            await _db.AuditLogs.AddAsync(log);
            await _db.SaveChangesAsync();
            _db.SetCurrentUser(null);
        }
    }
}