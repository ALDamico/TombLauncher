using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private string _title;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}