using System;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Xaml.Interactivity;

namespace TombLauncher.Behaviors;

public class EventToCommand : Behavior<StyledElement>
{
    static EventToCommand()
    {
        EventProperty.Changed.AddClassHandler<AvaloniaObject>(OnEventChanged);
    }

    private Delegate _handler;
    private EventInfo _oldEvent;

    // Event
    public string Event
    {
        get => GetValue(EventProperty);
        set => SetValue(EventProperty, value);
    }

    public static readonly StyledProperty<string> EventProperty =
        AvaloniaProperty.Register<EventToCommand, string>(nameof(Event));

    // Command
    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> CommandProperty =
        AvaloniaProperty.Register<EventToCommand, ICommand>(nameof(Command));

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly StyledProperty<object> CommandParameterProperty = AvaloniaProperty.Register<EventToCommand, object>("CommandParameter");

    // PassArguments (default: false)
    public bool PassArguments
    {
        get => (bool)GetValue(PassArgumentsProperty);
        set => SetValue(PassArgumentsProperty, value);
    }

    public static readonly StyledProperty<bool> PassArgumentsProperty =
        AvaloniaProperty.Register<EventToCommand, bool>("PassArguments");

    private static void OnEventChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        var beh = (EventToCommand)d;

        if (beh.AssociatedObject != null) // is not yet attached at initial load
            beh.AttachHandler((string)e.NewValue);
    }

    protected override void OnAttached()
    {
        AttachHandler(Event); // initial set
    }

    /// <summary>
    /// Attaches the handler to the event
    /// </summary>
    private void AttachHandler(string eventName)
    {
        // detach old event
        if (_oldEvent != null)
            _oldEvent.RemoveEventHandler(AssociatedObject, _handler);

        // attach new event
        if (!string.IsNullOrEmpty(eventName))
        {
            var ei = AssociatedObject.GetType().GetEvent(eventName);
            if (ei != null)
            {
                var mi = GetType().GetMethod("ExecuteCommand", BindingFlags.Instance | BindingFlags.NonPublic);
                _handler = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
                ei.AddEventHandler(AssociatedObject, _handler);
                _oldEvent = ei; // store to detach in case the Event property changes
            }
            else
                throw new ArgumentException(
                    $"The event '{eventName}' was not found on type '{AssociatedObject.GetType().Name}'");
        }
    }
    
    private void ExecuteCommand(object sender, EventArgs e)
    {
        var parameter = PassArguments ? e : CommandParameter;
        if (Command == null) return;

        if (Command.CanExecute(parameter))
            Command.Execute(parameter);
    }
}