using CodeReviewerApp.Interface;
using Octokit;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace CodeReviewerApp.ViewModels
{
    public class RepoBranchViewModel : ViewModelBase
    {
        private readonly IGitHubService _github;
        private readonly Action<ViewModelBase> _navigate;

        public ObservableCollection<Repository> Repositories { get; } = new();
        public ObservableCollection<Branch> Branches { get; } = new();

        private Repository? _selectedRepository;
        public ICommand NextCommand { get; }
        public Repository? SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                if (_selectedRepository != value)
                {
                    _selectedRepository = value;
                    OnPropertyChanged();
                    LoadBranchesAsync();
                    SelectedBranch = null; // Reset branch when repo changes
                    ((IRelayCommand)NextCommand).NotifyCanExecuteChanged();
                }
            }
        }

        private Branch? _selectedBranch;
        public Branch? SelectedBranch
        {
            get => _selectedBranch;
            set
            {
                if (_selectedBranch != value)
                {
                    _selectedBranch = value;
                    OnPropertyChanged();
                    ((IRelayCommand)NextCommand).NotifyCanExecuteChanged();
                }
            }
        }

        

        public RepoBranchViewModel(IGitHubService github, Action<ViewModelBase> navigate)
        {
            _github = github;
            _navigate = navigate;

            NextCommand = new RelayCommand<object>(
                _ =>
                {
                    if (SelectedRepository != null && SelectedBranch != null)
                    {
                        _navigate(new PrListViewModel(_github, SelectedRepository, SelectedBranch.Name, _navigate));
                    }
                },
                _ => SelectedRepository != null && SelectedBranch != null
            );

            _ = LoadRepositoriesAsync();
        }

        private async Task LoadRepositoriesAsync()
        {
            try
            {
                // Set your GitHub PAT securely as env var or get from a config
                string? token = Environment.GetEnvironmentVariable("GITHUB_PAT");
                if (string.IsNullOrWhiteSpace(token))
                    throw new Exception("GitHub token not found in environment variable GITHUB_PAT.");

                await _github.AuthenticateAsync(token);
                var repos = await _github.GetUserRepositoriesAsync();
                Repositories.Clear();
                foreach (var r in repos)
                    Repositories.Add(r);
            }
            catch (Exception ex)
            {
                // TODO: Show error to user (e.g. via a MessageBox or status property)
                System.Diagnostics.Debug.WriteLine("Failed to load repos: " + ex.Message);
            }
        }

        private async Task LoadBranchesAsync()
        {
            Branches.Clear();
            if (SelectedRepository == null)
                return;
            try
            {
                var branches = await _github.GetBranchesAsync(SelectedRepository.Owner.Login, SelectedRepository.Name);
                foreach (var b in branches)
                    Branches.Add(b);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load branches: " + ex.Message);
            }
        }
    }
}
