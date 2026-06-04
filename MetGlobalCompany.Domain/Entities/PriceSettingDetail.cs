using System.ComponentModel;
using System.Runtime.CompilerServices;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

public class PriceSettingDetail : BaseEntity, INotifyPropertyChanged
{
    private decimal _price;

    public int PriceSettingId { get; set; }
    public virtual PriceSetting PriceSetting { get; set; } = null!;

    public int NomenclatureId { get; set; }
    public virtual Nomenclature Nomenclature { get; set; } = null!;

    public int PriceTypeId { get; set; }
    public virtual PriceType PriceType { get; set; } = null!;

    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}