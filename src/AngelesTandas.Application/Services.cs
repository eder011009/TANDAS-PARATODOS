using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngelesTandas.Domain;
using AngelesTandas.Application.Dto;

namespace AngelesTandas.Application
{
    public interface ITandaService
    {
        Task<TandaDto> CreateTandaAsync(TandaCreateRequest request, string adminUserId);
        Task ActivateTandaAsync(Guid tandaId, string adminUserId);
        Task<List<TandaListItemDto>> GetTandasAsync();
    }

    public interface IPaymentService
    {
        Task<Guid> CreatePaymentAsync(Guid tandaId, string userId, decimal amount);
        Task ApprovePaymentAsync(Guid paymentId, string adminUserId);
        Task RejectPaymentAsync(Guid paymentId, string adminUserId, string reason);
        Task CompletePaymentAsync(Guid paymentId);
        Task UploadReceiptAsync(Guid paymentId, string uploaderUserId, string blobUri);
        Task<List<PendingPaymentItemDto>> GetPendingPaymentsAsync();
        Task<string> UploadStreamToBlobAsync(Stream stream, string fileName);
    }

    public interface IAuditService
    {
        Task LogActionAsync(string actorUserId, string action, string entity, string entityId, AuditSeverity severity = AuditSeverity.Info);
    }

    public interface ISecurityService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hash, string password);
        string Encrypt(string plain);
        string Decrypt(string cipher);
    }
}