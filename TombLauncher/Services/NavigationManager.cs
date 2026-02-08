using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Core.Navigation;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TombLauncher.Services;

public partial class NavigationManager : ObservableObject
{
    public void SetDefaultPage(INavigationTarget defaultPage)
    {
        _defaultPage = defaultPage;
    }

    private INavigationTarget _defaultPage;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    private INavigationTarget _currentPage;

    private readonly Stack<INavigationTarget> _history = new();

    public void GoBack()
    {
        if (_history.Count == 0)
        {
            _history.Push(_defaultPage);
            CurrentPage = _defaultPage;
            return;
        }

        _history.Pop();
        if (_history.Count > 0)
        {
            CurrentPage = _history.Peek();
        }
    }

    public bool CanGoBack => _history.Count > 1;

    public async Task StartNavigationAsync(Task<INavigationTarget> newPage)
    {
        var page = await newPage;
        await StartNavigationAsync(page);
    }

    public async Task StartNavigationAsync(INavigationTarget newPage)
    {
        _history.Clear();
        await NavigateTo(newPage);
    }

    public void RequestRefresh()
    {
        OnPropertyChanged(nameof(CurrentPage));
    }

    public async Task NavigateTo(Task<INavigationTarget> newPage)
    {
        var awaitedPage = await newPage;
        await NavigateTo(awaitedPage);
    }

    public Task NavigateTo(INavigationTarget newPage)
    {
        _history.Push(newPage);
        CurrentPage = newPage;
        OnPropertyChanged(nameof(CanGoBack));
        return Task.CompletedTask;
    }
}