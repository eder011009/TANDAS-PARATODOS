using System;
using System.Collections.Generic;

namespace AngelesTandas.Application.Dto
{
    public class TandaCreateRequest
    {
        public string Name { get; set; } = null!;
        public decimal AmountPerPerson { get; set; }
        public int NumberOfParticipants { get; set; }
    }

    public class TandaDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal AmountPerPerson { get; set; }
        public int NumberOfParticipants { get; set; }
        public string Status { get; set; } = "Draft";
    }

    public class TandaListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int CurrentParticipants { get; set; }
        public string Status { get; set; } = "Draft";
    }

    public class PendingPaymentItemDto
    {
        public Guid Id { get; set; }
        public string TandaName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ReceiptUri { get; set; }
    }
}