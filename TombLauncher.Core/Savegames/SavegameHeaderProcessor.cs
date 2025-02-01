using System.Collections.Concurrent;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Core.Savegames;

public class SavegameHeaderProcessor : IDisposable
{
    // Create an AutoResetEvent EventWaitHandle
    private readonly EventWaitHandle _eventWaitHandle = new AutoResetEvent(false);
    private readonly Thread _worker;
    private readonly ConcurrentQueue<string> fileNamesQueue = new();
    private readonly SavegameHeaderReader _savegameHeaderReader;
    public List<FileBackupDto> ProcessedFiles { get; }
    public int Delay { get; set; } = 500;

    public SavegameHeaderProcessor()
    {
        _savegameHeaderReader = new SavegameHeaderReader();
        ProcessedFiles = new List<FileBackupDto>();
        // Create worker thread
        _worker = new Thread(Work);
        // Start worker thread
        _worker.Start();
    }

    public void EnqueueFileName(string fileName)
    {
        if (Directory.Exists(fileName))
            return;
        // Enqueue the file name
        // This statement is secured by lock to prevent other thread to mess with queue while enqueuing file name
        fileNamesQueue.Enqueue(fileName);
        // Signal worker that file name is enqueued and that it can be processed
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
                _eventWaitHandle.WaitOne();
            }
        }
    }

    private void ProcessFile(string e)
    {
        Task.Delay(Delay).GetAwaiter().GetResult();
        var header = _savegameHeaderReader.ReadHeader(e);
        if (header == null)
            return;

        var dto = new FileBackupDto()
        {
            Data = File.ReadAllBytes(e),
            FileName = e,
            FileType = FileType.Savegame,
            BackedUpOn = DateTime.Now
        };
        ProcessedFiles.Add(dto);
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