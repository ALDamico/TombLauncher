using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia.Full;

namespace TombLauncher.Helpers;

/// <summary>
/// Provides attached properties for <see cref="MarkdownScrollViewer"/> that inject
/// a proportional typography scale into the control's Styles at runtime.
/// <list type="bullet">
///   <item><see cref="BaseFontSizeProperty"/> – body text size (pt); headings scale proportionally.</item>
///   <item><see cref="FontFamilyProperty"/> – font family to apply; defaults to <c>Georgia, serif</c>.</item>
/// </list>
/// Both properties are independent: changing either one triggers a full style rebuild.
/// </summary>
public static class MarkdownTypography
{
    private const double H1Ratio        = 2.00;
    private const double H2Ratio        = 1.55;
    private const double H3Ratio        = 1.33;
    private const double H4Ratio        = 1.22;
    private const double H5Ratio        = 1.11;
    private const double H6Ratio        = 1.00;
    private const double BlockquoteRatio = 0.88;

    // ── Attached properties ──────────────────────────────────────────────────

    public static readonly AttachedProperty<double> BaseFontSizeProperty =
        AvaloniaProperty.RegisterAttached<MarkdownScrollViewer, double>(
            "BaseFontSize",
            typeof(MarkdownTypography),
            defaultValue: 18.0);

    public static readonly AttachedProperty<FontFamily?> FontFamilyProperty =
        AvaloniaProperty.RegisterAttached<MarkdownScrollViewer, FontFamily?>(
            "FontFamily",
            typeof(MarkdownTypography),
            defaultValue: null);

    static MarkdownTypography()
    {
        BaseFontSizeProperty.Changed.AddClassHandler<MarkdownScrollViewer>(OnScaleChanged);
        FontFamilyProperty.Changed.AddClassHandler<MarkdownScrollViewer>(OnScaleChanged);
    }

    // ── Getters / Setters ────────────────────────────────────────────────────

    public static double GetBaseFontSize(MarkdownScrollViewer viewer)
        => viewer.GetValue(BaseFontSizeProperty);
    public static void SetBaseFontSize(MarkdownScrollViewer viewer, double value)
        => viewer.SetValue(BaseFontSizeProperty, value);

    public static FontFamily? GetFontFamily(MarkdownScrollViewer viewer)
        => viewer.GetValue(FontFamilyProperty);
    public static void SetFontFamily(MarkdownScrollViewer viewer, FontFamily? value)
        => viewer.SetValue(FontFamilyProperty, value);

    // ── Internal ─────────────────────────────────────────────────────────────

    private static void OnScaleChanged(MarkdownScrollViewer viewer, AvaloniaPropertyChangedEventArgs _)
        => ApplyTypographyScale(viewer);

    private static void ApplyTypographyScale(MarkdownScrollViewer viewer)
    {
        var baseSize = viewer.GetValue(BaseFontSizeProperty);
        var font     = viewer.GetValue(FontFamilyProperty); // null = inherit from tree

        viewer.Styles.Clear();

        viewer.Styles.Add(BodyStyle(font, baseSize));
        viewer.Styles.Add(HeadingStyle(font, "Heading1", Round(baseSize * H1Ratio), FontWeight.Bold));
        viewer.Styles.Add(HeadingStyle(font, "Heading2", Round(baseSize * H2Ratio), FontWeight.SemiBold));
        viewer.Styles.Add(HeadingStyle(font, "Heading3", Round(baseSize * H3Ratio), FontWeight.SemiBold));
        viewer.Styles.Add(HeadingStyle(font, "Heading4", Round(baseSize * H4Ratio), FontWeight.Medium));
        viewer.Styles.Add(HeadingStyle(font, "Heading5", Round(baseSize * H5Ratio), FontWeight.Medium));
        viewer.Styles.Add(HeadingStyle(font, "Heading6", Round(baseSize * H6Ratio), FontWeight.Medium));
        viewer.Styles.Add(BlockquoteStyle(font, Round(baseSize * BlockquoteRatio)));
    }

    private static Style BodyStyle(FontFamily? font, double size)
    {
        var s = new Style(x => x.OfType<CTextBlock>());
        AddSetters(s, font, size, FontWeight.Normal, FontStyle.Normal);
        return s;
    }

    private static Style HeadingStyle(FontFamily? font, string cls, double size, FontWeight weight)
    {
        var s = new Style(x => x.OfType<CTextBlock>().Class(cls));
        AddSetters(s, font, size, weight, FontStyle.Normal);
        return s;
    }

    private static Style BlockquoteStyle(FontFamily? font, double size)
    {
        var s = new Style(x => x.Class("Blockquote").Descendant().OfType<CTextBlock>());
        AddSetters(s, font, size, FontWeight.Normal, FontStyle.Italic);
        return s;
    }

    private static void AddSetters(Style s, FontFamily? font, double size,
        FontWeight weight, FontStyle style)
    {
        if (font != null)
            s.Setters.Add(new Setter(CTextBlock.FontFamilyProperty, font));
        s.Setters.Add(new Setter(CTextBlock.FontSizeProperty, size));
        s.Setters.Add(new Setter(CTextBlock.FontWeightProperty, weight));
        s.Setters.Add(new Setter(CTextBlock.FontStyleProperty, style));
    }

    private static double Round(double value)
        => Math.Round(value, MidpointRounding.AwayFromZero);
}
