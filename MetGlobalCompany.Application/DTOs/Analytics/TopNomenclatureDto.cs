namespace MetGlobalCompany.Application.DTOs.Analytics;

public class TopNomenclatureDto
{
    public string NomenclatureName { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public decimal PercentageOfTotal { get; set; }
}