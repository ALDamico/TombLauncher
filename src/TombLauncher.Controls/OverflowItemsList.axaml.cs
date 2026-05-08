using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;

namespace TombLauncher.Controls;

public partial class OverflowItemsList : UserControl
{
    public OverflowItemsList()
    {
        ItemsToShow = DefaultItemsToShow;
        InitializeComponent();
        _innerItemsControl = this.FindControl<ItemsControl>("InnerItemsControl")!;
    }

    private readonly ItemsControl _innerItemsControl;

    private const int DefaultItemsToShow = 10;

    public sealed class OverflowMarker
    {
        public int Count { get; init; }
    }

    public static readonly StyledProperty<DataTemplate> ItemTemplateProperty =
        AvaloniaProperty.Register<OverflowItemsList, DataTemplate>(nameof(ItemTemplate));

    public DataTemplate ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly StyledProperty<DataTemplate?> OverflowTemplateProperty =
        AvaloniaProperty.Register<OverflowItemsList, DataTemplate?>(nameof(OverflowTemplate));

    public DataTemplate? OverflowTemplate
    {
        get => GetValue(OverflowTemplateProperty);
        set => SetValue(OverflowTemplateProperty, value);
    }

public static readonly StyledProperty<ICollection?> ItemsProperty =
        AvaloniaProperty.Register<OverflowItemsList, ICollection?>(nameof(Items));

    public ICollection? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public static readonly StyledProperty<int> ItemsToShowProperty =
        AvaloniaProperty.Register<OverflowItemsList, int>(nameof(ItemsToShow));

    public int ItemsToShow
    {
        get => GetValue(ItemsToShowProperty);
        set => SetValue(ItemsToShowProperty, value);
    }

    private IEnumerable<object>? _displayedItems;

    protected static readonly DirectProperty<OverflowItemsList, IEnumerable<object>?> DisplayedItemsProperty =
        AvaloniaProperty.RegisterDirect<OverflowItemsList, IEnumerable<object>?>(
            nameof(DisplayedItems), o => o.DisplayedItems);

    protected IEnumerable<object>? DisplayedItems
    {
        get => _displayedItems;
        private set => SetAndRaise(DisplayedItemsProperty, ref _displayedItems, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsProperty || change.Property == ItemsToShowProperty)
            RebuildDisplayedItems();
        if (change.Property == ItemTemplateProperty || change.Property == OverflowTemplateProperty)
            UpdateItemTemplate();
    }

    private void UpdateItemTemplate()
    {
        _innerItemsControl.ItemTemplate = new FuncDataTemplate<object>((data, _) =>
            data is OverflowMarker
                ? OverflowTemplate?.Build(data) ?? new ContentControl { Content = data }
                : ItemTemplate?.Build(data) ?? new ContentControl { Content = data }
        );
    }

    private void RebuildDisplayedItems()
    {
        if (Items == null)
        {
            DisplayedItems = null;
            return;
        }

        var overflowCount = Math.Max(0, Items.Count - ItemsToShow);
        var items = Items.Cast<object>().Take(ItemsToShow).ToList();
        if (overflowCount > 0)
            items.Add(new OverflowMarker { Count = overflowCount });
        DisplayedItems = items;
    }
}
