namespace DocumentManager.Core.Models;

public sealed class ServiceRecord
{
    public long Id { get; set; }

    public DateTime Date { get; set; }

    public required string ServiceOrderFolio { get; set; }

    public required string InternalFolio { get; set; }

    public required string FinalPdfPath { get; set; }
}

