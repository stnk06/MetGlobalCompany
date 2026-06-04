using System.Collections.Generic;

namespace MetGlobalCompany.Application.DTOs.Analytics;

public class SalesDashboardDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalInvoices { get; set; }
    public decimal AverageReceipt { get; set; }
    public decimal TotalItemsSold { get; set; }
    public List<TopNomenclatureDto> TopNomenclatures { get; set; } = new();
    public List<TopContractorDto> TopContractors { get; set; } = new();
    public List<DailySalesDto> SalesByDate { get; set; } = new();
}