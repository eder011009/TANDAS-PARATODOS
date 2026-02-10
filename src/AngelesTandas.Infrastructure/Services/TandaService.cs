using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngelesTandas.Application;
using AngelesTandas.Application.Dto;
using AngelesTandas.Infrastructure.Data;
using AngelesTandas.Domain;
using Microsoft.EntityFrameworkCore;

namespace AngelesTandas.Infrastructure.Services
{
    public class TandaService : ITandaService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _auditService;

        public TandaService(ApplicationDbContext db, IAuditService auditService)
        {
            _db = db;
            _auditService = auditService;
        }

        public async Task<TandaDto> CreateTandaAsync(TandaCreateRequest request, string adminUserId)
        {
            // Validaciones de reglas del dominio
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.NumberOfParticipants <= 0)
                throw new ArgumentException("NumberOfParticipants must be greater than zero", nameof(request.NumberOfParticipants));

            var tanda = new Tanda
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                AmountPerPerson = request.AmountPerPerson,
                NumberOfParticipants = request.NumberOfParticipants,
                NumberOfTurns = request.NumberOfParticipants,
                TotalCollectedPerTurn = request.AmountPerPerson * request.NumberOfParticipants,
                StartDate = DateTime.UtcNow,
                Status = "Draft"
            };

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.SetCurrentUser(adminUserId);
                await _db.Tandas.AddAsync(tanda);
                await _db.SaveChangesAsync();

                await _auditService.LogActionAsync(adminUserId, "CreateTanda", nameof(Tanda), tanda.Id.ToString(), AuditSeverity.Info);

                await tx.CommitAsync();

                return new TandaDto { Id = tanda.Id, Name = tanda.Name, AmountPerPerson = tanda.AmountPerPerson, NumberOfParticipants = tanda.NumberOfParticipants, Status = tanda.Status };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            finally
            {
                _db.SetCurrentUser(null);
            }
        }

        public async Task ActivateTandaAsync(Guid tandaId, string adminUserId)
        {
            var tanda = await _db.Tandas.Include(t => t.Participants).FirstOrDefaultAsync(t => t.Id == tandaId);
            if (tanda == null) throw new InvalidOperationException("Tanda not found");

            // Solo admin (esto debería verificarse por rol en caller)
            if (tanda.Status != "Draft") throw new InvalidOperationException("Only Draft tandas can be activated");

            if (tanda.Participants == null || tanda.Participants.Count != tanda.NumberOfParticipants)
                throw new InvalidOperationException("Cannot activate without full participants");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.SetCurrentUser(adminUserId);
                tanda.Status = "Active";
                _db.Tandas.Update(tanda);
                await _db.SaveChangesAsync();

                await _auditService.LogActionAsync(adminUserId, "ActivateTanda", nameof(Tanda), tanda.Id.ToString(), AuditSeverity.Info);

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            finally
            {
                _db.SetCurrentUser(null);
            }
        }

        public async Task<List<TandaListItemDto>> GetTandasAsync()
        {
            var list = await _db.Tandas
                .Select(t => new TandaListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    CurrentParticipants = t.Participants == null ? 0 : t.Participants.Count,
                    Status = t.Status
                }).ToListAsync();

            return list;
        }
    }
}