using System;

namespace TombLauncher.ViewModels;

/// <summary>
/// Marks a property or backing field so that changes to it
/// do not set <see cref="SettingsSectionViewModelBase.IsChanged"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IgnoreChangesAttribute : Attribute;
