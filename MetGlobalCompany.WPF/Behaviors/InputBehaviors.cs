using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetGlobalCompany.WPF.Behaviors;

public static class InputBehaviors
{
    public static readonly DependencyProperty IsNumericOnlyProperty = DependencyProperty.RegisterAttached(
        "IsNumericOnly", typeof(bool), typeof(InputBehaviors), new PropertyMetadata(false, OnIsNumericOnlyChanged));

    public static bool GetIsNumericOnly(DependencyObject obj) => (bool)obj.GetValue(IsNumericOnlyProperty);
    public static void SetIsNumericOnly(DependencyObject obj, bool value) => obj.SetValue(IsNumericOnlyProperty, value);

    private static void OnIsNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.PreviewTextInput += TextBox_PreviewTextInput;
                DataObject.AddPastingHandler(textBox, TextBoxPasting);
            }
            else
            {
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                DataObject.RemovePastingHandler(textBox, TextBoxPasting);
            }
        }
    }

    private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !e.Text.All(char.IsDigit);

    private static void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            if (!text.All(char.IsDigit)) e.CancelCommand();
        }
        else e.CancelCommand();
    }

    public static readonly DependencyProperty IsPhoneMaskProperty = DependencyProperty.RegisterAttached(
        "IsPhoneMask", typeof(bool), typeof(InputBehaviors), new PropertyMetadata(false, OnIsPhoneMaskChanged));

    public static bool GetIsPhoneMask(DependencyObject obj) => (bool)obj.GetValue(IsPhoneMaskProperty);
    public static void SetIsPhoneMask(DependencyObject obj, bool value) => obj.SetValue(IsPhoneMaskProperty, value);

    private static void OnIsPhoneMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue) textBox.TextChanged += TextBox_PhoneTextChanged;
            else textBox.TextChanged -= TextBox_PhoneTextChanged;
        }
    }

    private static bool _isFormatting = false;

    private static void TextBox_PhoneTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isFormatting || sender is not TextBox textBox) return;
        _isFormatting = true;
        var caret = textBox.CaretIndex;
        var originalLength = textBox.Text.Length;
        var digits = new string(textBox.Text.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("7") || digits.StartsWith("8")) digits = digits.Substring(1);
        if (digits.Length > 10) digits = digits.Substring(0, 10);
        string formatted = "+7";
        if (digits.Length > 0) formatted += $" ({digits.Substring(0, Math.Min(3, digits.Length))}";
        if (digits.Length >= 3) formatted += $") {digits.Substring(3, Math.Min(3, digits.Length - 3))}";
        if (digits.Length >= 6) formatted += $" {digits.Substring(6, Math.Min(2, digits.Length - 6))}";
        if (digits.Length >= 8) formatted += $"-{digits.Substring(8, Math.Min(2, digits.Length - 8))}";
        if (digits.Length == 0 && textBox.Text.Length > 0) formatted = "";
        textBox.Text = formatted;
        var newCaret = caret + (textBox.Text.Length - originalLength);
        textBox.CaretIndex = Math.Max(0, Math.Min(newCaret, textBox.Text.Length));
        _isFormatting = false;
    }

    public static readonly DependencyProperty IsPositiveDecimalOnlyProperty = DependencyProperty.RegisterAttached(
        "IsPositiveDecimalOnly", typeof(bool), typeof(InputBehaviors), new PropertyMetadata(false, OnIsPositiveDecimalOnlyChanged));

    public static bool GetIsPositiveDecimalOnly(DependencyObject obj) => (bool)obj.GetValue(IsPositiveDecimalOnlyProperty);
    public static void SetIsPositiveDecimalOnly(DependencyObject obj, bool value) => obj.SetValue(IsPositiveDecimalOnlyProperty, value);

    private static void OnIsPositiveDecimalOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.PreviewTextInput += PositiveDecimal_PreviewTextInput;
                DataObject.AddPastingHandler(textBox, PositiveDecimal_Pasting);
            }
            else
            {
                textBox.PreviewTextInput -= PositiveDecimal_PreviewTextInput;
                DataObject.RemovePastingHandler(textBox, PositiveDecimal_Pasting);
            }
        }
    }

    private static void PositiveDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = (TextBox)sender;
        string currentText = textBox.Text;
        if (textBox.SelectionLength > 0)
        {
            currentText = currentText.Remove(textBox.SelectionStart, textBox.SelectionLength);
        }
        var fullText = currentText.Insert(textBox.CaretIndex, e.Text);
        e.Handled = !IsPositiveDecimal(fullText);
    }

    private static void PositiveDecimal_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            var textBox = (TextBox)sender;
            string currentText = textBox.Text;
            if (textBox.SelectionLength > 0)
            {
                currentText = currentText.Remove(textBox.SelectionStart, textBox.SelectionLength);
            }
            var fullText = currentText.Insert(textBox.CaretIndex, text);
            if (!IsPositiveDecimal(fullText)) e.CancelCommand();
        }
        else e.CancelCommand();
    }

    private static bool IsPositiveDecimal(string text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        text = text.Replace(',', '.');
        if (text.Count(c => c == '.') > 1) return false;
        return text.All(c => char.IsDigit(c) || c == '.');
    }
}