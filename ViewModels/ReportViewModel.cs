using CodeReviewerApp.Interface;
using CodeReviewerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace CodeReviewerApp.ViewModels
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly IGitHubService _github;
        public AIReport AiReport { get; }

        public ICommand BackCommand { get; }

        public ReportViewModel(AIReport aiReport, Action<ViewModelBase> navigate, IGitHubService github)
        {
            AiReport = aiReport ?? throw new ArgumentNullException(nameof(aiReport));
            _navigate = navigate;
            _github = github;

            // Back: you can customize where to go (PR diff or PR list, etc.)
            BackCommand = new RelayCommand<object>(_ =>
                _navigate(new RepoBranchViewModel(_github, _navigate))
            );
        }
    }
}
