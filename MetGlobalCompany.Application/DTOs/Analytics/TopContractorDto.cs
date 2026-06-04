namespace MetGlobalCompany.Application.DTOs.Analytics;

public class TopContractorDto
{
    public string ContractorName { get; set; } = string.Empty;
    public string Inn { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int InvoicesCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}