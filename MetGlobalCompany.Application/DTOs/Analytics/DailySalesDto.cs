using System;

namespace MetGlobalCompany.Application.DTOs.Analytics;

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int InvoicesCount { get; set; }
}