using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Core.Savegames;

public class SavegameHeaderProcessor
{
    // Create an AutoResetEvent EventWaitHandle
        private EventWaitHandle eventWaitHandle = new AutoResetEvent(false);
        private Thread worker;
        private readonly object locker = new();
        private Queue<string> fileNamesQueue = new();
        private SavegameHeaderReader _savegameHeaderReader;
        public List<FileBackupDto> ProcessedFiles { get; }

        public SavegameHeaderProcessor()
        {
            _savegameHeaderReader = new SavegameHeaderReader();
            ProcessedFiles = new List<FileBackupDto>();
            // Create worker thread
            worker = new Thread(Work);
            // Start worker thread
            worker.Start();
        }

        public void EnqueueFileName(string FileName)
        {
            if (Directory.Exists(FileName))
                return;
            // Enqueue the file name
            // This statement is secured by lock to prevent other thread to mess with queue while enqueuing file name
            Console.WriteLine($"Enqueueing {FileName}");
            lock (locker) fileNamesQueue.Enqueue(FileName);
            // Signal worker that file name is enqueued and that it can be processed
            eventWaitHandle.Set();
        }

        private void Work()
        {
            while (true)
            {
                string fileName = null;

                // Dequeue the file name
                lock (locker)
                    if (fileNamesQueue.Count > 0)
                    {
                        fileName = fileNamesQueue.Dequeue();
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
                    eventWaitHandle.WaitOne();
                }
            }
        }

        private void ProcessFile(string e)
        {
            Console.WriteLine($"{DateTime.Now} Processing {e}");
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
            Console.WriteLine($"{DateTime.Now} Done processing {e}");
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
            worker.Join();
            // Release any OS resources
            eventWaitHandle.Close();
        }

        #endregion
}