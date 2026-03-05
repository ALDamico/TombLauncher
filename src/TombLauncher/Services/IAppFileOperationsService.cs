using System.Threading.Tasks;

namespace TombLauncher.Services;

public interface IAppFileOperationsService
{
    Task CleanUpTempFiles();
    Task DeleteDirectory(string path);
    Task DeleteFile(string path);
}
