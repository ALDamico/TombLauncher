namespace TombLauncher.Patchers.Tomb4Plus.Models;

public record FurrOpcode
{
    public required List<byte[]> ByteArrays { get; set; }
    public required string FunctionName { get; set; }
    public bool ReverseArgs { get; set; }
    public required List<int> AddressTable { get; set; }
    public int TotalLength { get; set; }
    public string? FirstArgType { get; set; }
    public string? SecondArgType { get; set; }
}

public record FurrCommand
{
    public required string FunctionName { get; set; }
    public object? FirstArg { get; set; }
    public object? SecondArg { get; set; }

    public static FurrCommand UnknownCommand { get; } = new FurrCommand() { FunctionName = "UNKNOWN COMMAND" };
}

public record FurrOptimalCommand
{
    public FurrCommand? NewCommand { get; init; }
    public bool WasNop { get; init; }
}

public record FurrData
{
    public List<List<FurrCommand>> FurrFlipeffects { get; set; } = new();
    public List<List<FurrCommand>> FurrRacetimerEvents { get; set; } = new();
}