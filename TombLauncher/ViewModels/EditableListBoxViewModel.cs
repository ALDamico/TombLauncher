using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;

namespace TombLauncher.ViewModels;

public abstract partial class EditableListBoxViewModel : ObservableValidator 
{
    public EditableListBoxViewModel()
    {
        _editedValueIndex = -1;
        AddValueCmd = new RelayCommand(AddValue, CanAddValue);
        ClearCurrentValueCmd = new RelayCommand(ClearCurrentValue, CanClearCurrentValue);
        DeleteValueCmd = new RelayCommand<CheckableItem<string>>(DeleteValue, CanDeleteValue);
        EditValueCmd = new RelayCommand<CheckableItem<string>>(EditValue, CanEditValue);
        HandleKeyUpCmd = new RelayCommand<KeyEventArgs>(HandleKeyUp);
    }

    private CheckableItem<string> _editedValue;
    private int _editedValueIndex;
    private ObservableCollection<CheckableItem<string>> _targetCollection;

    public ObservableCollection<CheckableItem<string>> TargetCollection
    {
        get => _targetCollection;
        set
        {
            if (_targetCollection != null)
            {
                _targetCollection.CollectionChanged -= TargetCollectionOnCollectionChanged;
            }

            SetProperty(ref _targetCollection, value);
            if (value != null)
            {
                _targetCollection.CollectionChanged += TargetCollectionOnCollectionChanged;
            }
        }
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ClearCurrentValueCmd), nameof(AddValueCmd), nameof(EditValueCmd))]
    [CustomValidation(typeof(EditableListBoxViewModel), nameof(InvokeValidateMethod))]
    private string _currentValue;

    [ObservableProperty] private string _watermark;
    [ObservableProperty] private string _header;
    
    public IRelayCommand AddValueCmd { get; }

    private void AddValue()
    {
        TargetCollection.Add(new CheckableItem<string>(){IsChecked = true, Value = CurrentValue});
        CurrentValue = string.Empty;
        _editedValue = null;
    }

    private bool CanAddValue() => CurrentValue.IsNotNullOrWhiteSpace() && !HasErrors;
    
    public IRelayCommand ClearCurrentValueCmd { get; }

    private void ClearCurrentValue()
    {
        CurrentValue = string.Empty;
        if (_editedValue != null)
        {
            TargetCollection.Insert(_editedValueIndex, _editedValue);
            _editedValue = null;
            _editedValueIndex = -1;
        }
    }

    private bool CanClearCurrentValue() => CurrentValue.IsNotNullOrWhiteSpace();
    
    public IRelayCommand DeleteValueCmd { get; }

    private void DeleteValue(CheckableItem<string> valueToDelete)
    {
        TargetCollection.Remove(valueToDelete);
    }

    private bool CanDeleteValue(CheckableItem<string> valueToDelete)
    {
        return valueToDelete is { CanUserCheck: true };
    }
    
    public IRelayCommand EditValueCmd { get; }

    private void EditValue(CheckableItem<string> valueToEdit)
    {
        // TODO Add EditInProgress
        _editedValue = valueToEdit;
        _editedValueIndex = TargetCollection.IndexOf(valueToEdit);
        TargetCollection.Remove(valueToEdit);
        CurrentValue = valueToEdit.Value;
    }

    private bool CanEditValue(CheckableItem<string> valueToEdit)
    {
        return valueToEdit is { CanUserCheck: true } && _editedValue == null;
    }
    
    public IRelayCommand<KeyEventArgs> HandleKeyUpCmd { get; }

    private void HandleKeyUp(KeyEventArgs keyEventArgs)
    {
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
    
    private void TargetCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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