using TombLauncher.Core.Savegames;

namespace TombLauncher.Core.Dtos;

public class SavegameHeaderProxy : SavegameHeader
{
    public SavegameHeaderProxy(SavegameHeaderReader headerReader)
    {
        _headerReader = headerReader;
    }

    public override string Filepath
    {
        get => _filePath;
        set
        {
            _proxiedInstance = null;
            _filePath = value;
        }
    }

    public override string LevelName
    {
        get
        {
            InitProxy();
            return _proxiedInstance.LevelName;
        }
        set => _proxiedInstance.LevelName = value;
    }

    public override int SaveNumber
    {
        get
        {
            InitProxy();
            return _proxiedInstance.SaveNumber;
        }
        set => _proxiedInstance.SaveNumber = value;
    }

    public override int SlotNumber
    {
        get
        {
            InitProxy();
            return _proxiedInstance.SlotNumber;
        }
        set => _proxiedInstance.SlotNumber = value;
    }

    private void InitProxy()
    {
        if (_proxiedInstance == null)
        {
            _proxiedInstance = _headerReader.ReadHeader(_filePath);
        }
    }

    private string _filePath;

    private SavegameHeader _proxiedInstance;

    private readonly SavegameHeaderReader _headerReader;
}