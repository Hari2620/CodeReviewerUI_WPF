using System.ComponentModel;
using System.Runtime.CompilerServices;
using CodeReviewerApp.ViewModels;
using CodeReviewerApp.Interface;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            if (_currentViewModel != value)
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }
    }

    public MainWindowViewModel(IGitHubService github)
    {
        // Pass the Navigate method as a delegate
        CurrentViewModel = new RepoBranchViewModel(github, Navigate);
    }

    public void Navigate(ViewModelBase nextViewModel)
    {
        CurrentViewModel = nextViewModel;
    }
}
