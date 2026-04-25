using Avalonia.Controls;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Views.Pages;

public partial class WelcomePageView : UserControl
{
    public WelcomePageView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is WelcomePageViewModel vm)
            {
                vm.ScrollToRandomSuggestionRequested += () =>
                    RandomSuggestionSection.BringIntoView();
            }
        };
    }
}