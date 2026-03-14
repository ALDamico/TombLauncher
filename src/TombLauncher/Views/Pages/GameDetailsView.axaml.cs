using Avalonia.Controls;
using Markdown.Avalonia;
using Markdown.Avalonia.Html;

namespace TombLauncher.Views.Pages;

public partial class GameDetailsView : UserControl
{
    public GameDetailsView()
    {
        InitializeComponent();

        var viewer = this.FindControl<MarkdownScrollViewer>("DescriptionViewer");
        viewer?.Plugins?.Plugins?.Add(new HtmlPlugin());
    }
}