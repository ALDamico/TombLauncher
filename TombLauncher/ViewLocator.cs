using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TombLauncher.ViewModels;

namespace TombLauncher;

public class ViewLocator : IDataTemplate
{
    private const string ViewModelsPrefix = "TombLauncher.ViewModels.ViewModels.";
    private const string ViewsPrefix = "TombLauncher.Views.";
    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        
        

        var pageControl = GetControl(data, "Page");
        if (pageControl != null)
        {
            return pageControl;
        }
        
        var view = GetControl(data, "View");
        if (view != null)
        {
            return view;
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            var control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    private Control GetControl(object data, string targetReplacement)
    {
        var name = data.GetType().Name!.Replace("ViewModel", targetReplacement, StringComparison.Ordinal);
        name = ViewsPrefix + name;
        var type = Type.GetType(name);

        if (type != null)
        {
            var control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }

        return null;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}