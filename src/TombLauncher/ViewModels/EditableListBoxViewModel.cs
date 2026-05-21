using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts;

namespace TombLauncher.ViewModels;

public abstract partial class EditableListBoxViewModel : ObservableValidator
{
    private CheckableItem<string>? _editedValue;
    private int _editedValueIndex = -1;

    public ObservableCollection<CheckableItem<string>>? TargetCollection
    {
        get;
        set
        {
            if (field != null)
            {
                field.CollectionChanged -= TargetCollectionOnCollectionChanged;
            }

            SetProperty(ref field, value);
            if (field != null)
            {
                field.CollectionChanged += TargetCollectionOnCollectionChanged;
            }
        }
    } = null!;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ClearCurrentValueCommand), nameof(AddValueCommand), nameof(EditValueCommand))]
    [CustomValidation(typeof(EditableListBoxViewModel), nameof(InvokeValidateMethod))]
    private string _currentValue = null!;

    [ObservableProperty] private string _watermark = null!;
    [ObservableProperty] private string _header = null!;
    [ObservableProperty] private PackIconRemixIconKind? _headerIcon;

    [RelayCommand(CanExecute = nameof(CanAddValue))]
    private void AddValue()
    {
        TargetCollection ??= [];
        TargetCollection.Add(new CheckableItem<string>() { IsChecked = true, Value = CurrentValue });
        CurrentValue = string.Empty;
        _editedValue = null;
    }

    private bool CanAddValue() => CurrentValue.IsNotNullOrWhiteSpace() && !HasErrors;

    [RelayCommand(CanExecute = nameof(CanClearCurrentValue))]
    private void ClearCurrentValue()
    {
        CurrentValue = string.Empty;
        if (_editedValue != null)
        {
            TargetCollection?.Insert(_editedValueIndex, _editedValue);
            _editedValue = null;
            _editedValueIndex = -1;
        }
    }

    private bool CanClearCurrentValue() => CurrentValue.IsNotNullOrWhiteSpace();

    [RelayCommand(CanExecute = nameof(CanDeleteValue))]
    private void DeleteValue(CheckableItem<string>? valueToDelete)
    {
        if (valueToDelete != null)
        {
            TargetCollection?.Remove(valueToDelete);
        }
    }

    private bool CanDeleteValue(CheckableItem<string>? valueToDelete)
    {
        return valueToDelete is { CanUserCheck: true };
    }

    [RelayCommand(CanExecute = nameof(CanEditValue))]
    private void EditValue(CheckableItem<string>? valueToEdit)
    {
        if (valueToEdit == null) return;

        // TODO Add EditInProgress
        _editedValue = valueToEdit;
        if (TargetCollection != null)
        {
            _editedValueIndex = TargetCollection.IndexOf(valueToEdit);
            TargetCollection.Remove(valueToEdit);
        }
        CurrentValue = valueToEdit.Value;
    }

    private bool CanEditValue(CheckableItem<string>? valueToEdit)
    {
        return valueToEdit is { CanUserCheck: true } && _editedValue == null;
    }

    [RelayCommand]
    private void HandleKeyUp(KeyEventArgs? keyEventArgs)
    {
        if (keyEventArgs == null) return;
        switch (keyEventArgs.Key)
        {
            case Key.Enter:
                var validationResult = Validate(CurrentValue);
                if (validationResult == ValidationResult.Success && CanAddValue())
                    AddValue();
                break;
            case Key.Escape:
                ClearCurrentValue();
                break;
        }
    }

    private void TargetCollectionOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TargetCollection));
    }

    public static ValidationResult InvokeValidateMethod(string newValue, ValidationContext validationContext)
    {
        var instance = (EditableListBoxViewModel)validationContext.ObjectInstance;
        return instance.Validate(newValue);
    }

    public abstract ValidationResult Validate(string newValue);
}