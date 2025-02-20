using System;
using System.Diagnostics;
using TombLauncher.Contracts.Enums;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels;

[DebuggerDisplay("{Description}")]
public class EnumViewModel<T> : ViewModelBase where T: Enum
{
    public EnumViewModel(T value)
    {
        Value = value;
        OnPropertyChanged(nameof(Description));
    }
    public T Value { get; }
    public string Description => Value.GetDescription();
}