namespace TombLauncher.Contracts.Navigation;

public interface INavigableViewModel
{
    Task OnNavigatingTo(object parameter);
    Task OnNavigatedTo(object parameter);
}
