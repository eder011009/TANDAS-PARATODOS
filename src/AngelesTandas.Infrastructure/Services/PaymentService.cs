using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngelesTandas.Application;
using AngelesTandas.Application.Dto;
using AngelesTandas.Infrastructure.Data;
using AngelesTandas.Domain;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace AngelesTandas.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;

        public PaymentService(ApplicationDbContext db, IAuditService auditService, IConfiguration configuration)
        {
            _db = db;
            _auditService = auditService;
            _configuration = configuration;
        }

        public async Task<Guid> CreatePaymentAsync(Guid tandaId, string userId, decimal amount)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                TandaId = tandaId,
                UserId = userId,
                Amount = amount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.SetCurrentUser(userId);
                await _db.Payments.AddAsync(payment);
                await _db.SaveChangesAsync();

                await _auditService.LogActionAsync(userId, "CreatePayment", nameof(Payment), payment.Id.ToString(), AuditSeverity.Info);

                await tx.CommitAsync();
                return payment.Id;
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

        public async Task UploadReceiptAsync(Guid paymentId, string uploaderUserId, string blobUri)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) throw new InvalidOperationException("Payment not found");

            var receipt = new PaymentReceipt
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentId,
                FilePath = blobUri ?? string.Empty,
                BlobUri = blobUri,
                UploadedAt = DateTime.UtcNow,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = uploaderUserId
            };

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.SetCurrentUser(uploaderUserId);
                await _db.PaymentReceipts.AddAsync(receipt);
                payment.Status = "Pending";
                _db.Payments.Update(payment);
                await _db.SaveChangesAsync();

                await _auditService.LogActionAsync(uploaderUserId, "UploadReceipt", nameof(PaymentReceipt), receipt.Id.ToString(), AuditSeverity.Info);

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

        public async Task<string> UploadStreamToBlobAsync(Stream stream, string fileName)
        {
            var connectionString = _configuration.GetConnectionString("BlobStorage") ?? _configuration["BlobStorage:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Blob storage connection string is not configured");

            var containerName = _configuration["BlobStorage:Container"] ?? "receipts";
            var blobServiceClient = new BlobServiceClient(connectionString);
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync();

            var blobClient = container.GetBlobClient(fileName);
            stream.Position = 0;
            await blobClient.UploadAsync(stream, overwrite: true);
            return blobClient.Uri.ToString();
        }

        public async Task ApprovePaymentAsync(Guid paymentId, string adminUserId)
        {
            var payment = await _db.Payments.Include(p => p.Receipt).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) throw new InvalidOperationException("Payment not found");
            if (payment.Status != "Pending") throw new InvalidOperationException("Only pending payments can be approved");

            if (payment.Receipt == null || !payment.Receipt.IsVerified)
                throw new InvalidOperationException("Cannot approve payment without a verified receipt");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.SetCurrentUser(adminUserId);
                payment.Status = "Approved";
                payment.ApprovedAt = DateTime.UtcNow;

                _db.Payments.Update(payment);
                await _db.SaveChangesAsync();

                await _auditService.LogActionAsync(adminUserId, "ApprovePayment", nameof(Payment), payment.Id.ToString(), AuditSeverity.Info);

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

        public async Task RejectPaymentAsync(Guid paymentId, string adminUserId, string reason)
        {
            var payment = await _db.Payments.Include(p => p.Receipt).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) throw new InvalidOperationException("Payment not found");
            if (payment.Status != "Pending") throw new InvalidOperationException("Only pending payments can be rejected");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.SetCurrentUser(adminUserId);
                payment.Status = "Rejected";
                _db.Payments.Update(payment);

                await _auditService.LogActionAsync(adminUserId, "RejectPayment", nameof(Payment), payment.Id.ToString(), AuditSeverity.Warning);

                await _db.SaveChangesAsync();
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

        public async Task CompletePaymentAsync(Guid paymentId)
        {
            var payment = await _db.Payments.Include(p => p.Receipt).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) throw new InvalidOperationException("Payment not found");
            if (payment.Status != "Approved") throw new InvalidOperationException("Only approved payments can be completed");

            if (payment.Receipt == null || !payment.Receipt.IsVerified)
                throw new InvalidOperationException("Cannot complete payment without a verified receipt");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.SetCurrentUser(payment.CreatedBy);

                payment.Status = "Completed";
                _db.Payments.Update(payment);

                var turn = await _db.Turns.FirstOrDefaultAsync(t => t.TandaId == payment.TandaId && t.UserId == payment.UserId && !t.IsPaidOut);
                if (turn != null)
                {
                    turn.IsPaidOut = true;
                    _db.Turns.Update(turn);
                }

                await _db.SaveChangesAsync();

                await _auditService.LogActionAsync(payment.CreatedBy ?? "system", "CompletePayment", nameof(Payment), payment.Id.ToString(), AuditSeverity.Info);

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

        public async Task<List<PendingPaymentItemDto>> GetPendingPaymentsAsync()
        {
            var list = await _db.Payments
                .Where(p => p.Status == "Pending")
                .Include(p => p.Receipt)
                .Select(p => new PendingPaymentItemDto
                {
                    Id = p.Id,
                    TandaName = _db.Tandas.Where(t => t.Id == p.TandaId).Select(t => t.Name).FirstOrDefault() ?? string.Empty,
                    UserName = p.UserId,
                    Amount = p.Amount,
                    ReceiptUri = p.Receipt != null ? (p.Receipt.BlobUri ?? p.Receipt.FilePath) : null
                }).ToListAsync();

            return list;
        }
    }
}