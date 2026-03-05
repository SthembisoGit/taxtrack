namespace TaxTrack.Domain.Entities;

public sealed class FinancialTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid UploadJobId { get; set; }
    public required string SourceRecordId { get; set; }
    public DateOnly TransactionDate { get; set; }
    public required string LedgerCategory { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public decimal VatAmount { get; set; }
    public required string Direction { get; set; }
    public required string SourceSystem { get; set; }
    public string? TaxInvoiceNumber { get; set; }
    public string? SupplierVatNumber { get; set; }
    public DateOnly? TaxInvoiceDate { get; set; }
    public string? Vat201Reference { get; set; }
}
