using CodeReviewerApp.Interface;
using Octokit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Input;
using Newtonsoft.Json;
using CodeReviewerApp.Helpers;
using System.Configuration;
using CodeReviewerApp.Models;

namespace CodeReviewerApp.ViewModels
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly IGitHubService _github;
        private readonly Repository _repo;
        private readonly PullRequest _pullRequest;
        private readonly Action<ViewModelBase> _navigate;

        public ICommand AnalyzeCommand { get; }
        private string _aiFeedback;
        public string AiFeedback
        {
            get => _aiFeedback;
            set { _aiFeedback = value; OnPropertyChanged(); }
        }

        private string _diffText;
        public string DiffText
        {
            get => _diffText;
            set { _diffText = value; OnPropertyChanged(); }
        }

        private AIReport _aiReport;
        public AIReport AiReport
        {
            get => _aiReport;
            set { _aiReport = value; OnPropertyChanged(); }
        }

        public PullRequest PullRequest => _pullRequest;
        public ICommand BackCommand { get; }

        public DiffViewModel(
            IGitHubService github,
            Repository repo,
            PullRequest pullRequest,
            Action<ViewModelBase> navigate)
        {
            _github = github;
            _repo = repo;
            _pullRequest = pullRequest;
            _navigate = navigate;

            BackCommand = new RelayCommand<object>(_ =>
                _navigate(new PrListViewModel(_github, _repo, _pullRequest.Base.Ref, _navigate))
            );

            AnalyzeCommand = new RelayCommand<object>(_ => AnalyzeWithAI());

            _ = LoadDiffAsync();
        }

        private async Task LoadDiffAsync()
        {
            try
            {
                DiffText = "Loading diff...";
                DiffText = await _github.GetPullRequestDiffAsync(_repo.Owner.Login, _repo.Name, _pullRequest.Number);
                Debug.WriteLine($"DIFF LOADED. Length: {DiffText?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                DiffText = $"Error loading diff: {ex.Message}";
                Debug.WriteLine("Diff load failed: " + ex);
            }
        }

        private async void AnalyzeWithAI()
        {
            AiFeedback = "Analyzing...";
            try
            {
                // Build payload for the new API (repo_url, pr_number, base)
                var payloadObj = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "repo_url", _repo.HtmlUrl },
                    { "pr_number", _pullRequest.Number },
                    { "base", _pullRequest.Base.Ref }
                };
                string jsonPayload = JsonConvert.SerializeObject(payloadObj);
                string apiUrl = ConfigurationManager.AppSettings["AIReviewApiUrl"];
                var response = await HttpHelper.PostJsonAsync(apiUrl, jsonPayload);

                // Parse API response to AIReport model
                AiReport = JsonConvert.DeserializeObject<AIReport>(response);

                // Show pretty JSON (for debug or user)
                AiFeedback = JsonConvert.SerializeObject(AiReport, Formatting.Indented);

                // Navigate to report screen
                _navigate(new ReportViewModel(this.AiReport, _navigate, _github));
            }
            catch (Exception ex)
            {
                AiFeedback = $"Error: {ex.Message}";
            }
        }
    }
}
