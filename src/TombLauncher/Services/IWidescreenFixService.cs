using System.Collections.Generic;
using System.Threading.Tasks;

namespace TombLauncher.Services;

public interface IWidescreenFixService
{
    Task ApplyWidescreenFix(int gameId);
    Task RemoveWidescreenFix(int gameId);
    Task<bool> IsWidescreenFixApplied(int gameId);
    List<string> GetAvailableWidescreenFixes();
}