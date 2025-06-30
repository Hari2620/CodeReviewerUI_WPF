using CodeReviewerApp.Interface;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReviewerApp.Service
{
    public class GitHubService : IGitHubService
    {
        private GitHubClient _client;

        public Task AuthenticateAsync(string token)
        {
            _client = new GitHubClient(new ProductHeaderValue("CodeReviewerApp"))
            {
                Credentials = new Credentials(token)
            };
            return Task.CompletedTask;
        }

        public async Task<List<Branch>> GetBranchesAsync(string owner, string repo)
        {
            return (await _client.Repository.Branch.GetAll(owner, repo)).ToList();
        }

        public async Task<List<PullRequest>> GetPullRequestsAsync(string owner, string repo)
        {
            return (await _client.PullRequest.GetAllForRepository(owner, repo, new PullRequestRequest()
            {
                State = ItemStateFilter.Open
            })).ToList();
        }

        public async Task<string> GetPullRequestDiffAsync(string owner, string repo, int prNumber)
        {
            var url = new Uri($"https://api.github.com/repos/{owner}/{repo}/pulls/{prNumber}");
            var response = await _client.Connection.Get<string>(
                url,
                new Dictionary<string, string>(),
                "application/vnd.github.v3.diff"
            );
            return response.Body;
        }
        public async Task<List<Repository>> GetUserRepositoriesAsync()
        {
            var repos = await _client.Repository.GetAllForCurrent();
            return repos.ToList();
        }
    }
}
