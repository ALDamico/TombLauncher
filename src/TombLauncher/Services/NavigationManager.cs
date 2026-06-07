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
    public partial INavigableViewModel? CurrentPage { get; set; }

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
        var nextViewModel = await Task.Run(() => _serviceProvider.GetRequiredService<TViewModel>());

        try
        {
            await nextViewModel.OnNavigatingTo(parameter!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnNavigatingTo for {ViewModelType}", nextViewModel.GetType().Name);
        }

        await _navigationLock.WaitAsync();
        try
        {
            if (CurrentPage != null)
                _history.Push(CurrentPage);

            CurrentPage = nextViewModel;
        }
        finally
        {
            _navigationLock.Release();
        }

        await Task.Delay(NavigationConstants.TransitionDuration + TimeSpan.FromMilliseconds(100));

        try
        {
            await nextViewModel.OnNavigatedTo(parameter!).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnNavigatedTo for {ViewModelType}", nextViewModel.GetType().Name);
        }
    }

    public async Task GoBack()
    {
        INavigableViewModel? previousPage;

        await _navigationLock.WaitAsync();
        try
        {
            _history.TryPop(out previousPage);
            if (previousPage != null)
                CurrentPage = previousPage;
        }
        finally
        {
            _navigationLock.Release();
        }

        if (previousPage != null)
        {
            await Task.Delay(NavigationConstants.TransitionDuration + TimeSpan.FromMilliseconds(100));
            try
            {
                await previousPage.OnNavigatedTo(null!);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatedTo during GoBack for {ViewModelType}", previousPage.GetType().Name); }
        }
    }

    public async Task NavigateToRoot(Type viewModelType, object? parameter = null)
    {
        var nextViewModel = (INavigableViewModel)_serviceProvider.GetRequiredService(viewModelType);

        try
        {
            await nextViewModel.OnNavigatingTo(parameter!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnNavigatingTo during NavigateToRoot for {ViewModelType}", nextViewModel.GetType().Name);
        }

        await _navigationLock.WaitAsync();
        try
        {
            _history.Clear();
            OnPropertyChanged(nameof(CanGoBack));
            CurrentPage = nextViewModel;
        }
        finally
        {
            _navigationLock.Release();
        }

        await Task.Delay(NavigationConstants.TransitionDuration + TimeSpan.FromMilliseconds(100));

        try
        {
            await nextViewModel.OnNavigatedTo(parameter!);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error in OnNavigatedTo during NavigateToRoot(Type) for {ViewModelType}", nextViewModel.GetType().Name); }
    }
}