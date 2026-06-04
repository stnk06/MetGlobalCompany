using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MetGlobalCompany.WPF.Behaviors;

/// <summary>
/// Поведение для TextBlock, реализующее подсветку совпадений при поиске.
/// </summary>
public static class TextHighlightBehavior
{
    public static readonly DependencyProperty HighlightTextProperty = DependencyProperty.RegisterAttached(
        "HighlightText",
        typeof(string),
        typeof(TextHighlightBehavior),
        new PropertyMetadata(string.Empty, OnHighlightTextChanged));

    public static string GetHighlightText(DependencyObject obj)
    {
        return (string)obj.GetValue(HighlightTextProperty);
    }

    public static void SetHighlightText(DependencyObject obj, string value)
    {
        obj.SetValue(HighlightTextProperty, value);
    }

    private static void OnHighlightTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock) return;

        string originalText = textBlock.Text;
        string highlightText = e.NewValue as string ?? string.Empty;

        if (string.IsNullOrEmpty(originalText) || string.IsNullOrEmpty(highlightText))
        {
            ResetTextBlock(textBlock, originalText);
            return;
        }

        int index = originalText.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);

        if (index < 0)
        {
            ResetTextBlock(textBlock, originalText);
            return;
        }

        textBlock.Inlines.Clear();

        string preText = originalText.Substring(0, index);
        string matchText = originalText.Substring(index, highlightText.Length);
        string postText = originalText.Substring(index + highlightText.Length);

        if (!string.IsNullOrEmpty(preText))
        {
            textBlock.Inlines.Add(new Run(preText));
        }

        textBlock.Inlines.Add(new Run(matchText)
        {
            Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)), 
            FontWeight = FontWeights.Bold
        });

        if (!string.IsNullOrEmpty(postText))
        {
            textBlock.Inlines.Add(new Run(postText));
        }
    }

    private static void ResetTextBlock(TextBlock textBlock, string text)
    {
        textBlock.Inlines.Clear();
        textBlock.Inlines.Add(new Run(text ?? string.Empty));
    }
}