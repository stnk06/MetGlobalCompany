using System.ComponentModel;
using System.Runtime.CompilerServices;
using MetGlobalCompany.Domain.Common;

namespace MetGlobalCompany.Domain.Entities;

public class OrderDetail : BaseEntity, INotifyPropertyChanged
{
    private decimal _quantity;
    private decimal _price;
    private decimal _sum;

    public int OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public int NomenclatureId { get; set; }
    public virtual Nomenclature Nomenclature { get; set; } = null!;

    public decimal Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(); RecalculateSum(); }
    }

    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(); RecalculateSum(); }
    }

    public decimal Sum
    {
        get => _sum;
        set { _sum = value; OnPropertyChanged(); }
    }

    private void RecalculateSum()
    {
        Sum = Quantity * Price;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}