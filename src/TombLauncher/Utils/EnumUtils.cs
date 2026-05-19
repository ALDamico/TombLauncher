using System;
using System.Collections.Generic;
using System.Linq;
using TombLauncher.Contracts.Enums;
using TombLauncher.ViewModels;

namespace TombLauncher.Utils;

public static class EnumUtils
{
    public static IEnumerable<EnumViewModel<T>> GetEnumViewModels<T>() where T: struct, Enum
    {
        return Enum.GetValues<T>().Select(e => new EnumViewModel<T>(e));
    }

    /// <summary>
    /// Strips variant flags (bits 6+) from a composite engine value, returning the canonical base engine.
    /// Engine variants like <see cref="GameEngine.Tr1x"/> encode their base engine in the lower 6 bits
    /// (e.g. Tr1x = TombRaider1 | variant bits), so masking with 0b111111 isolates the base.
    /// </summary>
    public static GameEngine GetBaseEngine(this GameEngine engine) => engine & (GameEngine)0b111111;
}