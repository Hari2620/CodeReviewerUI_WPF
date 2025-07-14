using CodeReviewerApp.Helpers;
using CodeReviewerApp.Interface;
using CodeReviewerApp.Models;
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
        public async Task<List<ChangedFileModel>> GetPullRequestFilesAsync(string owner, string repo, int pullRequestNumber)
        {
            string? token = Environment.GetEnvironmentVariable("GITHUB_PAT");
            var url = $"https://api.github.com/repos/{owner}/{repo}/pulls/{pullRequestNumber}/files";
            var json = await HttpHelper.GetAsync(url, token);

            // Use Newtonsoft.Json to parse
            dynamic fileList = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var files = new List<ChangedFileModel>();

            foreach (var file in fileList)
            {
                files.Add(new ChangedFileModel
                {
                    FileName = file.filename,
                    Status = file.status,
                    RawUrl = file.raw_url // This gives the raw content for the head branch version
                });
            }
            return files;
        }

    }
}
