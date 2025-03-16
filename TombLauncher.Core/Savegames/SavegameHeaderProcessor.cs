using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Savegames.HeaderReaders;

namespace TombLauncher.Core.Savegames;

public class SavegameHeaderProcessor : IDisposable
{
    private ILogger<SavegameHeaderProcessor> _logger =
        Ioc.Default.GetRequiredService<ILogger<SavegameHeaderProcessor>>();
    // Create an AutoResetEvent EventWaitHandle
    private EventWaitHandle _eventWaitHandle = new AutoResetEvent(false);
    private readonly Thread _worker;
    private readonly ConcurrentQueue<string> fileNamesQueue = new();
    public ISavegameHeaderReader SavegameHeaderReader { get; set; }
    public List<SavegameBackupDto> ProcessedFiles { get; }
    public int Delay { get; set; } = 500;

    public SavegameHeaderProcessor()
    {
        _logger.LogInformation("Header processor initialized");
        ProcessedFiles = new List<SavegameBackupDto>();
        // Create worker thread
        _worker = new Thread(Work);
        // Start worker thread
        _worker.Start();
        _logger.LogInformation("Savegame header processor worker started");
    }

    public void EnqueueFileName(string fileName)
    {
        if (Directory.Exists(fileName))
        {
            _logger.LogWarning("{Filename} was a directory. Will be ignored", fileName);
            return;
        }
        // Enqueue the file name
        // This statement is secured by lock to prevent other thread to mess with queue while enqueuing file name
        fileNamesQueue.Enqueue(fileName);
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
        while (true)
        {
            string fileName = null;

            // Dequeue the file name
            //lock (locker)
            if (fileNamesQueue.Count > 0)
            {
                fileNamesQueue.TryDequeue(out fileName);
                // If file name is null then stop worker thread
                if (fileName == null) return;
            }

            if (fileName != null)
            {
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
        var header = SavegameHeaderReader.ReadHeader(e);
        if (header == null)
            return;

        var dto = new SavegameBackupDto()
        {
            Data = File.ReadAllBytes(e),
            FileName = e,
            FileType = FileType.Savegame,
            BackedUpOn = DateTime.Now,
            LevelName = header.LevelName,
            SaveNumber = header.SaveNumber,
            SlotNumber = header.SlotNumber
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
        // Signal the FileProcessor to exit
        EnqueueFileName(null);
        // Wait for the FileProcessor's thread to finish
        _worker.Join();
        // Release any OS resources
        _eventWaitHandle.Close();
    }

    #endregion
}