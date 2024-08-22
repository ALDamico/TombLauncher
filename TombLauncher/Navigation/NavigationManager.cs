using System;
using System.Collections.Generic;
using TombLauncher.ViewModels;

namespace TombLauncher.Navigation;

public class NavigationManager
{
    public NavigationManager(PageViewModel defaultPage)
    {
        _defaultPage = defaultPage;
    }

    private readonly PageViewModel _defaultPage;
    private PageViewModel _currentPage;
    
    public Stack<PageViewModel> History { get; set; } = new();

    public void GoBack()
    {
        if (History.Count == 0)
        {
            History.Push(_defaultPage);
            _currentPage = _defaultPage;
            InvokeOnNavigated();
            return;
        }

        History.Pop();
        if (History.Count > 0)
        {
            _currentPage = History.Peek();
        }
        InvokeOnNavigated();
    }

    public bool CanGoBack()
    {
        return History.Count > 1;
    }
    
    public PageViewModel GetCurrentPage() => _currentPage;

    public void StartNavigation(PageViewModel newPage)
    {
        History.Clear();
        NavigateTo(newPage);
    }

    public void NavigateTo(PageViewModel newPage)
    {
        History.Push(newPage);
        _currentPage = newPage;
        InvokeOnNavigated();
    }
    
    public event Action OnNavigated;

    private void InvokeOnNavigated()
    {
        OnNavigated?.Invoke();
    }
}