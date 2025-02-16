using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TombLauncher.ViewModels;

namespace TombLauncher.Navigation;

public class NavigationManager
{
    internal void SetDefaultPage(PageViewModel defaultPage)
    {
        _defaultPage = defaultPage;
    }

    private PageViewModel _defaultPage;
    private PageViewModel _currentPage;

    private readonly Stack<PageViewModel> _history = new();

    public void GoBack()
    {
        if (_history.Count == 0)
        {
            _history.Push(_defaultPage);
            _currentPage = _defaultPage;
            InvokeOnNavigated();
            return;
        }

        _history.Pop();
        if (_history.Count > 0)
        {
            _currentPage = _history.Peek();
        }
        InvokeOnNavigated();
    }

    public bool CanGoBack()
    {
        return _history.Count > 1;
    }
    
    public PageViewModel GetCurrentPage() => _currentPage;

    public async Task StartNavigationAsync(Task<PageViewModel> newPage)
    {
        var page = await newPage;
        await StartNavigationAsync(page);
    }

    public async Task StartNavigationAsync(PageViewModel newPage)
    {
        _history.Clear();
        await NavigateTo(newPage);
    }

    public void RequestRefresh()
    {
        InvokeOnNavigated();
    }

    public async Task NavigateTo(Task<PageViewModel> newPage)
    {
        var awaitedPage = await newPage;
        await NavigateTo(awaitedPage);
    }

    public Task NavigateTo(PageViewModel newPage)
    {
        _history.Push(newPage);
        _currentPage = newPage;
        InvokeOnNavigated();
        return Task.CompletedTask;
    }
    
    public event Action OnNavigated;

    private async void InvokeOnNavigated()
    {
        await Task.Run(() => OnNavigated?.Invoke());
    }
}