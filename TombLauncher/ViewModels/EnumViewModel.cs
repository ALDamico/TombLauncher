using System;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels;

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