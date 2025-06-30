using CodeReviewerApp.Interface;
using Octokit;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace CodeReviewerApp.ViewModels
{
    public class PrListViewModel : ViewModelBase
    {
        private readonly IGitHubService _github;
        private readonly Repository _repo;
        private readonly string _branch;
        private readonly Action<ViewModelBase> _navigate;
        public ICommand OpenDiffCommand { get; }
        public ObservableCollection<PullRequest> PullRequests { get; } = new();
        private PullRequest? _selectedPullRequest;
        public PullRequest? SelectedPullRequest
        {
            get => _selectedPullRequest;
            set
            {
                if (_selectedPullRequest != value)
                {
                    _selectedPullRequest = value;
                    OnPropertyChanged();
                    ((IRelayCommand)OpenDiffCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public ICommand BackCommand { get; }

        public PrListViewModel(IGitHubService github, Repository repo, string branch, Action<ViewModelBase> navigate)
        {
            _github = github;
            _repo = repo;
            _branch = branch;
            _navigate = navigate;

            BackCommand = new RelayCommand<object>(_ =>
                _navigate(new RepoBranchViewModel(_github, _navigate))
            );

            OpenDiffCommand = new RelayCommand<object>(
                _ =>
                {
                    if (SelectedPullRequest != null)
                    {
                        _navigate(new DiffViewModel(_github, _repo, SelectedPullRequest, _navigate));
                    }
                },
                _ => SelectedPullRequest != null
            );

            _ = LoadPullRequestsAsync();
        }

        private async Task LoadPullRequestsAsync()
        {
            try
            {
                Debug.WriteLine($"Loading PRs for {_repo.Owner.Login}/{_repo.Name}...");
                var prs = await _github.GetPullRequestsAsync(_repo.Owner.Login, _repo.Name);

                // Always clear/add on UI thread
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        PullRequests.Clear();
                        foreach (var pr in prs)
                            PullRequests.Add(pr);

                        Debug.WriteLine($"Total PRs loaded (UI thread): {PullRequests.Count}");
                    });
                }
                else
                {
                    PullRequests.Clear();
                    foreach (var pr in prs)
                        PullRequests.Add(pr);

                    Debug.WriteLine($"Total PRs loaded: {PullRequests.Count}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load pull requests: " + ex.Message);
            }
        }
    }
}
