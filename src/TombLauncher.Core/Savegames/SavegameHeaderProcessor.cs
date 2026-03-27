using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Exceptions;
using TombLauncher.Core.Savegames.HeaderReaders;
using TombLauncher.Core.Utils;

namespace TombLauncher.Core.Savegames;

public class SavegameHeaderProcessor : IDisposable
{
    private readonly ILogger<SavegameHeaderProcessor> _logger;
    // Create an AutoResetEvent EventWaitHandle
    private EventWaitHandle _eventWaitHandle = new AutoResetEvent(false);
    private Thread? _worker;
    private readonly ConcurrentQueue<string> _fileNamesQueue = new();
    public ISavegameHeaderReader SavegameHeaderReader { get; set; } = null!;
    public List<SavegameBackupDto> ProcessedFiles { get; }
    public int Delay { get; init; } = 500;
    public bool ErrorOccurred { get; private set; }
    private bool _isRunning;
    private bool _disposed;
    private volatile bool _stopRequested;

    public SavegameHeaderProcessor(ILogger<SavegameHeaderProcessor> logger)
    {
        _logger = logger;
        _logger.LogInformation("Header processor initialized");
        ProcessedFiles = new List<SavegameBackupDto>();
    }

    /// <summary>
    /// Starts the background worker thread. Should only be called when a game is launched.
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;
        _stopRequested = false;
        _worker = new Thread(Work);
        _worker.Start();
        _isRunning = true;
        _logger.LogInformation("Savegame header processor worker started");
    }

    public void EnqueueFileName(string fileName)
    {
        if (fileName.EndsWith("savereg.tmp"))
            return;
        if (Directory.Exists(fileName))
        {
            _logger.LogWarning("{Filename} was a directory. Will be ignored", fileName);
            return;
        }
        // Enqueue the file name
        // This statement is secured by lock to prevent other thread to mess with queue while enqueuing file name
        _fileNamesQueue.Enqueue(fileName);
        // Signal worker that file name is enqueued and that it can be processed
        if (_eventWaitHandle.SafeWaitHandle.IsClosed)
        {
            _logger.LogError("AutoResetEvent was closed. Reinitializing...");
            _eventWaitHandle = new AutoResetEvent(false);
            _logger.LogError("AutoResetEvent reinitialized");
        }
        _eventWaitHandle.Set();
    }

    private void Work()
    {
        while (!_stopRequested)
        {
            // Dequeue the file name
            if (_fileNamesQueue.TryDequeue(out var fileName))
            {
                if (ErrorOccurred)
                    continue;
                // Process file
                ProcessFile(fileName);
            }
            else
            {
                // No more file names - wait for a signal
                if (_eventWaitHandle.SafeWaitHandle.IsClosed)
                {
                    _logger.LogError("AutoResetEvent was closed. Reinitializing...");
                    _eventWaitHandle = new AutoResetEvent(false);
                    _logger.LogError("AutoResetEvent reinitialized");
                }
                _eventWaitHandle.WaitOne();
            }
        }
    }

    private void ProcessFile(string e)
    {
        Task.Delay(Delay).GetAwaiter().GetResult();
        SavegameHeader? header = null;
        try
        {
            header = SavegameHeaderReader.ReadHeader(e);
        }
        catch (SavegameParseException ex)
        {
            _logger.LogError("An error occurred while processing file {Filename}: {Exception}", e, ex);
            ErrorOccurred = true;
        }

        if (header == null)
            return;

        var fileBytes = File.ReadAllBytes(e);
        var md5 = Md5Utils.ComputeMd5Hash(fileBytes);
        var dto = new SavegameBackupDto()
        {
            Data = fileBytes,
            FileName = e,
            FileType = FileType.Savegame,
            BackedUpOn = DateTime.Now,
            LevelName = header.LevelName,
            SaveNumber = header.SaveNumber,
            SlotNumber = header.SlotNumber,
            Md5 = md5
        };
        ProcessedFiles.Add(dto);
        _logger.LogInformation("Added file {Filename} to queue", e);
    }

    public void ClearProcessedFiles()
    {
        ProcessedFiles.Clear();
    }


    #region IDisposable Members

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_worker != null && _isRunning)
        {
            // Signal the worker thread to stop
            _stopRequested = true;
            _eventWaitHandle.Set();
            // Wait for the worker thread to finish
            _worker.Join();
        }

        _isRunning = false;
        // Release any OS resources
        _eventWaitHandle.Close();
    }

    #endregion
}