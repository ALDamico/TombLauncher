namespace TombLauncher.Core.Exceptions;

public class SavegameParseException : Exception
{
    public SavegameParseException(Exception cause) : base("An error occurred while attempting to parse a savegame", cause)
    {
        
    }
}