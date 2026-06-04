using System.Windows;
using System.Windows.Controls;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.WPF.ViewModels;

namespace MetGlobalCompany.WPF.Views;

public partial class NomenclatureView : UserControl
{
    public NomenclatureView()
    {
        InitializeComponent();

        CategoriesTreeView.SelectedItemChanged += CategoriesTreeView_SelectedItemChanged;
    }

    private void CategoriesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is NomenclatureViewModel viewModel)
        {
            viewModel.SetSelectedCategory(e.NewValue as NomenclatureCategory);
        }
    }
}