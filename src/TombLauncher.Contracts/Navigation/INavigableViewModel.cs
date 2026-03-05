using System.Threading.Tasks;

namespace TombLauncher.Contracts.Navigation;

public interface INavigableViewModel
{
    Task OnNavigatedTo(object parameter);
    Task OnNavigatingFrom();
}
