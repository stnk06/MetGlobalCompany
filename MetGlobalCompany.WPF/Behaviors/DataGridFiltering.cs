using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace MetGlobalCompany.WPF.Behaviors;

public static class DataGridFiltering
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(DataGridFiltering), new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static readonly Dictionary<DataGrid, Dictionary<string, string>> _gridFilters = new();

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        if ((bool)e.NewValue)
        {
            grid.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnFilterTextChanged));
            grid.Unloaded += Grid_Unloaded;
            _gridFilters[grid] = new Dictionary<string, string>();
        }
        else
        {
            grid.RemoveHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnFilterTextChanged));
            grid.Unloaded -= Grid_Unloaded;
            _gridFilters.Remove(grid);
        }
    }

    private static void Grid_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is DataGrid grid)
        {
            _gridFilters.Remove(grid);
        }
    }

    private static void OnFilterTextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.OriginalSource is not TextBox textBox) return;

        var header = FindAncestor<DataGridColumnHeader>(textBox);
        if (header == null || header.Column == null) return;

        var grid = FindAncestor<DataGrid>(header);
        if (grid == null || !_gridFilters.ContainsKey(grid)) return;

        string propertyName = header.Column.SortMemberPath;
        if (string.IsNullOrEmpty(propertyName)) return;

        string filterText = textBox.Text.Trim();
        var filters = _gridFilters[grid];

        if (string.IsNullOrEmpty(filterText))
            filters.Remove(propertyName);
        else
            filters[propertyName] = filterText;

        ApplyFilters(grid);
    }

    private static void ApplyFilters(DataGrid grid)
    {
        ICollectionView view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
        if (view == null) return;

        var filters = _gridFilters[grid];

        if (filters.Count == 0)
        {
            view.Filter = null;
            return;
        }

        view.Filter = item =>
        {
            if (item == null) return false;

            foreach (var kvp in filters)
            {
                var value = GetPropertyValue(item, kvp.Key);
                if (value == null) return false;

                string stringValue = value.ToString() ?? string.Empty;
                if (stringValue.IndexOf(kvp.Value, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }
            return true;
        };
    }

    private static object? GetPropertyValue(object? obj, string propertyPath)
    {
        if (obj == null) return null;

        foreach (var part in propertyPath.Split('.'))
        {
            if (obj == null) return null;
            var type = obj.GetType();
            var info = type.GetProperty(part);
            if (info == null) return null;
            obj = info.GetValue(obj, null);
        }
        return obj;
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T ancestor) return ancestor;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        while (current != null);
        return null;
    }
}