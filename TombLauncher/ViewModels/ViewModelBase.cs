using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected ViewModelBase()
    {
        InitCmd = new RelayCommand(Init);
    }
    
    public ICommand InitCmd { get; set; }

    private void Init()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        RaiseInitialize();
    }
    
    private bool _isInitialized;
    
    public event Action Initialize;
    private void RaiseInitialize()
    {
        var handler = Initialize;
        handler?.Invoke();
    }
}