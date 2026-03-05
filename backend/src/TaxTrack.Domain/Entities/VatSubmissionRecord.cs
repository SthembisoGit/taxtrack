namespace TaxTrack.Domain.Entities;

public sealed class VatSubmissionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid UploadJobId { get; set; }
    public required string SourceRecordId { get; set; }
    public DateOnly TaxPeriodStart { get; set; }
    public DateOnly TaxPeriodEnd { get; set; }
    public decimal OutputVat { get; set; }
    public decimal InputVat { get; set; }
    public decimal VatRefundClaimed { get; set; }
    public decimal DeclaredTurnover { get; set; }
    public string Currency { get; set; } = "ZAR";
    public DateOnly SubmissionDate { get; set; }
    public required string Vat201Reference { get; set; }
    public DateOnly PaymentDate { get; set; }
    public required string PaymentReference { get; set; }
}
