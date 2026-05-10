using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Patchers;
using TombLauncher.Core.Patchers;
using TombLauncher.Patchers.Shared;

namespace TombLauncher.ViewModels.Pages.Patchers;

public partial class PatcherPageViewModel : PageViewModel
{
    public PatcherPageViewModel()
    {
        _progressLogger = new ProgressLogger(new Progress<LogEntry>(msg =>
        {
            Log.Add(msg);
            IsLogExpanded = true;
        }));
    }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApplyPatch))]
    [NotifyPropertyChangedFor(nameof(CanRevertPatch))]
    [NotifyCanExecuteChangedFor(nameof(ApplyPatchCommand))]
    [NotifyCanExecuteChangedFor(nameof(RevertPatchCommand))]
    IPatcherParametersViewModel _content = null!;

    private readonly ProgressLogger _progressLogger;
    private IGameMetadataLite? _gameMetadata;

    partial void OnContentChanged(IPatcherParametersViewModel? oldValue, IPatcherParametersViewModel? newValue)
    {
        if (oldValue != null)
            oldValue.PropertyChanged -= OnContentPropertyChanged;
        if (newValue != null)
            newValue.PropertyChanged += OnContentPropertyChanged;

        newValue?.Init(_gameMetadata, _progressLogger);
    }

    private void OnContentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IPatcherParametersViewModel.CanApplyPatch) or nameof(IPatcherParametersViewModel.CanRevertPatch))
        {
            OnPropertyChanged(nameof(CanApplyPatch));
            OnPropertyChanged(nameof(CanRevertPatch));
            ApplyPatchCommand.NotifyCanExecuteChanged();
            RevertPatchCommand.NotifyCanExecuteChanged();
        }
    }

    [ObservableProperty] private ObservableCollection<LogEntry> _log = new();
    [ObservableProperty] private bool _isLogExpanded;

    public bool CanApplyPatch => Content?.CanApplyPatch == true;
    public bool CanRevertPatch => Content?.CanRevertPatch == true;

    [RelayCommand(CanExecute = nameof(CanApplyPatch))]
    private async Task ApplyPatch()
    {
        if (Content == null!)
            return;

        await Content.ApplyPatch();
    }

    [RelayCommand(CanExecute = nameof(CanRevertPatch))]
    private async Task RevertPatch()
    {
        if (Content == null!)
            return;

        await Content.RevertPatch();
    }

    public override async Task OnNavigatedTo(object parameter)
    {
        _gameMetadata = (IGameMetadataLite)parameter;
        if (Content == null!)
            return;

        await Content.Init(_gameMetadata, _progressLogger);
    }
}