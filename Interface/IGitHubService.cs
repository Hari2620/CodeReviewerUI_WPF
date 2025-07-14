using CodeReviewerApp.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReviewerApp.Interface
{
    public interface IGitHubService
    {
        Task AuthenticateAsync(string token);
        Task<List<Repository>> GetUserRepositoriesAsync();
        Task<List<Branch>> GetBranchesAsync(string owner, string repo);
        Task<List<PullRequest>> GetPullRequestsAsync(string owner, string repo);
        Task<string> GetPullRequestDiffAsync(string owner, string repo, int prNumber);
        Task<List<ChangedFileModel>> GetPullRequestFilesAsync(string owner, string repo, int pullRequestNumber);
    }
}
