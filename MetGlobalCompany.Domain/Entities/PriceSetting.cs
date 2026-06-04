using System.Collections.Generic;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

public class PriceSetting : BaseDocument
{
    public string Comment { get; set; } = string.Empty;
    public virtual ICollection<PriceSettingDetail> Details { get; set; } = new List<PriceSettingDetail>();
}