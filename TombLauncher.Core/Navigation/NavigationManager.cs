namespace TombLauncher.Core.Navigation;

public class NavigationManager
{
    public void SetDefaultPage(INavigationTarget defaultPage)
    {
        _defaultPage = defaultPage;
    }

    private INavigationTarget _defaultPage;
    private INavigationTarget _currentPage;

    private readonly Stack<INavigationTarget> _history = new();

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
    
    public INavigationTarget GetCurrentPage() => _currentPage;

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
        InvokeOnNavigated();
    }

    public async Task NavigateTo(Task<INavigationTarget> newPage)
    {
        var awaitedPage = await newPage;
        await NavigateTo(awaitedPage);
    }

    public Task NavigateTo(INavigationTarget newPage)
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