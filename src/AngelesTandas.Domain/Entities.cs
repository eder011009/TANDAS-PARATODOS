using System;
using System.Collections.Generic;

namespace AngelesTandas.Domain
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }

    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        string? CreatedBy { get; set; }
        DateTime? ModifiedAt { get; set; }
        string? ModifiedBy { get; set; }
    }

    public enum AuditSeverity { Info = 0, Warning = 1, Critical = 2 }

    // Audit
    public class AuditLog : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? ActorUserId { get; set; }
        public string Action { get; set; } = null!;
        public string Entity { get; set; } = null!;
        public string EntityId { get; set; } = string.Empty;
        public AuditSeverity Severity { get; set; } = AuditSeverity.Info;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    // Profile (linked to Identity user by UserId string)
    public class Profile : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? RFC { get; set; }         // Encriptar en capa SecurityService
        public string? CURP { get; set; }        // Encriptar en capa SecurityService
        public string? AccountNumber { get; set; } // Encriptar
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    // Tanda context
    public class Tanda : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public decimal AmountPerPerson { get; set; }
        public int NumberOfParticipants { get; set; }
        public int NumberOfTurns { get; set; }
        public decimal TotalCollectedPerTurn { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; } = "Draft"; // Draft | Active | Completed | Cancelled
        public bool IsDeleted { get; set; } = false;

        public List<TandaParticipant> Participants { get; set; } = new();
        public List<Turn> Turns { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class TandaParticipant : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TandaId { get; set; }
        public string UserId { get; set; } = null!;
        public int TurnNumber { get; set; }
        public bool HasReceivedPayout { get; set; }
        public string Status { get; set; } = "Active"; // Active | Left | Defaulted
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class Turn : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TandaId { get; set; }
        public int TurnNumber { get; set; }
        public string UserId { get; set; } = null!;
        public string RandomnessToken { get; set; } = null!; // SHA256(orden + timestamp + salt)
        public bool IsPaidOut { get; set; }
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class Payment : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TandaId { get; set; }
        public string UserId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending | Approved | Completed | Rejected
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public PaymentReceipt? Receipt { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class PaymentReceipt : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PaymentId { get; set; }
        public string FilePath { get; set; } = null!; // legacy fallback
        public string? BlobUri { get; set; } // Azure Blob storage uri
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsVerified { get; set; }
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class PaymentInstructionTemplate : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = null!; // e.g., "DefaultPaymentInstruction"
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class NotificationTemplate : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = null!; // e.g., "PaymentPendingEmail"
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class CommissionRequest : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TandaId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class CommissionResponse : IAuditable, ISoftDelete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CommissionRequestId { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}