namespace TaxTrack.Domain.Entities;

public sealed class PayrollRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid UploadJobId { get; set; }
    public required string SourceRecordId { get; set; }
    public DateOnly PayPeriod { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalPaye { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalUif { get; set; }
    public decimal TotalSdl { get; set; }
    public string Currency { get; set; } = "ZAR";
    public DateOnly Emp201Period { get; set; }
    public decimal Emp201DeclaredPaye { get; set; }
    public decimal Emp201DeclaredUif { get; set; }
    public decimal Emp201DeclaredSdl { get; set; }
    public required string Emp501Reference { get; set; }
    public decimal Irp5TotalPaye { get; set; }
    public DateOnly SubmissionDate { get; set; }
    public DateOnly PaymentDate { get; set; }
    public required string PaymentReference { get; set; }
}
