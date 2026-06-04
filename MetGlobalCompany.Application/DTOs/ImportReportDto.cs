using System.Collections.Generic;

namespace MetGlobalCompany.Application.DTOs;

public class ImportReportDto
{
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ErrorsCount { get; set; }
    public List<string> Errors { get; set; } = new();
}