using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Navigation;

namespace TombLauncher.Services;

public partial class NavigationManager : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NavigationManager> _logger;
    private readonly ConcurrentStack<INavigableViewModel> _history = new();

    // Using a semaphore to ensure navigation operations are serialized even if called from multiple threads
    private readonly SemaphoreSlim _navigationLock = new(1, 1);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    private INavigableViewModel? _currentPage;

    public bool CanGoBack => _history.Count > 0;

    public NavigationManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<NavigationManager>>();
    }

    /// <summary>
    /// Navigates to a ViewModel type by resolving it from DI.
    /// Supports passing a navigation parameter.
    /// </summary>
    /// <summary>
    /// Navigates to a ViewModel type by resolving it from DI.
    /// Supports passing a navigation parameter.
    /// </summary>
    public async Task NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class, INavigableViewModel
    {
        var nextViewModel = _serviceProvider.GetRequiredService<TViewModel>();
        INavigableViewModel? previousPage;

        await _navigationLock.WaitAsync();
        try
        {
            previousPage = CurrentPage;

            if (previousPage != null)
            {
                _history.Push(previousPage);
            }

            // Update state (UI triggers happen here)
            CurrentPage = nextViewModel;
        }
        finally
        {
            _navigationLock.Release();
        }

        if (previousPage != null)
        {
            // Notify previous page it's about to be hidden
            // We do this outside the lock to avoid deadlocks if OnNavigatingFrom triggers navigation
            try
            {
                await previousPage.OnNavigatingFrom();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnNavigatingFrom for {ViewModelType}", previousPage.GetType().Name);
            }
        }

        // Initialize new page
        // We do this outside the lock to avoid deadlocks and allow UI to render the new page (likely in "Busy" state)
        try
        {
            await nextViewModel.OnNavigatedTo(parameter!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnNavigatedTo for {ViewModelType}", nextViewModel.GetType().Name);
        }
    }

    public async Task GoBack()
    {
        INavigableViewModel? previousPage = null;
        INavigableViewModel? pageNavigatingFrom = null;

        await _navigationLock.WaitAsync();
        try
        {
            if (_history.TryPop(out previousPage))
            {
                pageNavigatingFrom = CurrentPage;
                CurrentPage = previousPage;
            }
        }
        finally
        {
            _navigationLock.Release();
        }

        if (previousPage != null)
        {
            if (pageNavigatingFrom != null)
            {
                try
                {
                    await pageNavigatingFrom.OnNavigatingFrom();
                }
                catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatingFrom during GoBack for {ViewModelType}", pageNavigatingFrom.GetType().Name); }
            }

            // Re-activating the previous page
            try
            {
                await previousPage.OnNavigatedTo(null!);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatedTo during GoBack for {ViewModelType}", previousPage.GetType().Name); }
        }
    }

    public async Task NavigateToRoot<TViewModel>(object? parameter = null) where TViewModel : class, INavigableViewModel
    {
        var nextViewModel = _serviceProvider.GetRequiredService<TViewModel>();
        INavigableViewModel? previousPage;

        await _navigationLock.WaitAsync();
        try
        {
            _history.Clear();
            OnPropertyChanged(nameof(CanGoBack));

            previousPage = CurrentPage;
            CurrentPage = nextViewModel;
        }
        finally
        {
            _navigationLock.Release();
        }

        if (previousPage != null)
        {
            try
            {
                await previousPage.OnNavigatingFrom();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatingFrom during NavigateToRoot for {ViewModelType}", previousPage.GetType().Name); }
        }

        try
        {
            await nextViewModel.OnNavigatedTo(parameter!);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatedTo during NavigateToRoot for {ViewModelType}", nextViewModel.GetType().Name); }
    }

    public async Task NavigateTo(Type viewModelType, object? parameter = null)
    {
        var nextViewModel = (INavigableViewModel)_serviceProvider.GetRequiredService(viewModelType);
        INavigableViewModel? previousPage;

        await _navigationLock.WaitAsync();
        try
        {
            previousPage = CurrentPage;
            if (previousPage != null)
            {
                _history.Push(previousPage);
            }

            CurrentPage = nextViewModel;
        }
        finally
        {
            _navigationLock.Release();
        }

        if (previousPage != null)
        {
            try
            {
                await previousPage.OnNavigatingFrom();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatingFrom during NavigateTo(Type) for {ViewModelType}", previousPage.GetType().Name); }
        }

        try
        {
            await nextViewModel.OnNavigatedTo(parameter!);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatedTo during NavigateTo(Type) for {ViewModelType}", nextViewModel.GetType().Name); }
    }

    public async Task NavigateToRoot(Type viewModelType, object? parameter = null)
    {
        var nextViewModel = (INavigableViewModel)_serviceProvider.GetRequiredService(viewModelType);
        INavigableViewModel? previousPage;

        await _navigationLock.WaitAsync();
        try
        {
            _history.Clear();
            OnPropertyChanged(nameof(CanGoBack));

            previousPage = CurrentPage;
            CurrentPage = nextViewModel;
        }
        finally
        {
            _navigationLock.Release();
        }

        if (previousPage != null)
        {
            try
            {
                await previousPage.OnNavigatingFrom();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatingFrom during NavigateToRoot(Type) for {ViewModelType}", previousPage.GetType().Name); }
        }

        try
        {
            await nextViewModel.OnNavigatedTo(parameter!);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatedTo during NavigateToRoot(Type) for {ViewModelType}", nextViewModel.GetType().Name); }
    }
}