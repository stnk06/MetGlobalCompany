using System;
using System.Collections.Generic;

namespace MetGlobalCompany.Application.DTOs.Analytics;

// 1. Единый контейнер отчета
public class SalesReportDto
{
    public DateTime ReportDate { get; set; } = DateTime.Now;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public decimal GrandTotalQuantity { get; set; }
    public decimal GrandTotalSum { get; set; }

    public List<SalesReportContractorGroupDto> ContractorGroups { get; set; } = new();
}

// 2. Группировка Уровень 1 (Покупатель)
public class SalesReportContractorGroupDto
{
    public string ContractorName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalSum { get; set; }

    public List<SalesReportNomenclatureGroupDto> NomenclatureGroups { get; set; } = new();
}

// 3. Группировка Уровень 2 (Номенклатура)
public class SalesReportNomenclatureGroupDto
{
    public string NomenclatureName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalSum { get; set; }

    public List<SalesReportItemDto> Items { get; set; } = new();
}

// 4. Детальные записи отчета
public class SalesReportItemDto
{
    public DateTime DocumentDate { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Sum { get; set; }
}