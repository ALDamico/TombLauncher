using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

[DebuggerDisplay("{Description}")]
public partial class EnumViewModel<T> : ObservableObject where T: Enum
{
    public EnumViewModel(T value)
    {
        Value = value;
        OnPropertyChanged(nameof(Description));
    }
    public T Value { get; }
    public string Description => IsDisabled ? "-----" : Value.GetDescription();
    [ObservableProperty]
    public partial bool IsDisabled { get; set; }
}