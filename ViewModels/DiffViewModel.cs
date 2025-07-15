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
using System.Collections.ObjectModel;
using System.Linq;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex;
using System.Collections.Generic;

namespace CodeReviewerApp.ViewModels
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly IGitHubService _github;
        private readonly Repository _repo;
        private readonly PullRequest _pullRequest;
        private readonly Action<ViewModelBase> _navigate;

        public ObservableCollection<ChangedFileModel> ChangedFiles { get; set; } = new();
        public ObservableCollection<DiffRow> DiffRows { get; set; } = new();

        // Track applied suggestions by (filename, lineNumber) and store stripped text
        private readonly Dictionary<(string fileName, int lineNumber), string> _appliedCodeMap = new();

        public ObservableCollection<InlineAISuggestion> AppliedSuggestions { get; set; } = new ObservableCollection<InlineAISuggestion>();

        private string _aiSuggestion;
        public string AISuggestion
        {
            get => _aiSuggestion;
            set { _aiSuggestion = value; OnPropertyChanged(); }
        }

        private Guid _currentFileToken = Guid.NewGuid();

        private ChangedFileModel _selectedFile;
        public ChangedFileModel SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged();

                    // Bump token for async ops
                    _currentFileToken = Guid.NewGuid();
                    var token = _currentFileToken;

                    _ = LoadSelectedDiffAsync(token);

                    if (_selectedFile != null && !_selectedFile.IsOverall)
                        _ = FetchAISuggestionAsync(token);
                    else
                        AISuggestion = string.Empty;
                }
            }
        }

        public ICommand AnalyzeCommand { get; }
        public ICommand ApplyInlineSuggestionCommand { get; }
        public ICommand UndoInlineSuggestionCommand { get; }
        public ICommand OpenPopupCommand { get; }

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
        private string _leftFileText;
        public string LeftFileText
        {
            get => _leftFileText;
            set { _leftFileText = value; OnPropertyChanged(); }
        }
        private string _rightFileText;
        public string RightFileText
        {
            get => _rightFileText;
            set { _rightFileText = value; OnPropertyChanged(); }
        }

        private string _overallDiffText;

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
            ApplyInlineSuggestionCommand = new RelayCommand<InlineAISuggestion>(ApplyInlineSuggestion);
            UndoInlineSuggestionCommand = new RelayCommand<InlineAISuggestion>(UndoInlineSuggestion);
            OpenPopupCommand = new RelayCommand<DiffRow>(OpenPopupForRow);

            _ = LoadDiffAndFilesAsync();
        }

        private async Task LoadDiffAndFilesAsync()
        {
            try
            {
                DiffText = "Loading diff...";
                _overallDiffText = await _github.GetPullRequestDiffAsync(_repo.Owner.Login, _repo.Name, _pullRequest.Number);
                DiffText = _overallDiffText;

                var changedFiles = await _github.GetPullRequestFilesAsync(_repo.Owner.Login, _repo.Name, _pullRequest.Number);
                ChangedFiles.Clear();
                ChangedFiles.Add(new ChangedFileModel
                {
                    FileName = "Overall Diff",
                    Status = "all",
                    IsOverall = true
                });
                foreach (var file in changedFiles)
                {
                    ChangedFiles.Add(file);
                }
                SelectedFile = ChangedFiles.FirstOrDefault();
            }
            catch (Exception ex)
            {
                DiffText = $"Error loading diff: {ex.Message}";
            }
        }

        private async Task LoadSelectedDiffAsync(Guid token)
        {
            var thisToken = token;

            if (SelectedFile == null) return;

            if (SelectedFile.IsOverall)
            {
                if (thisToken != _currentFileToken) return;
                DiffText = _overallDiffText;
                LeftFileText = string.Empty;
                RightFileText = string.Empty;
                DiffRows.Clear();
            }
            else
            {
                DiffText = string.Empty;
                var baseSha = _pullRequest.Base.Sha;
                var left = await FetchFileContentFromBase(_repo.Owner.Login, _repo.Name, SelectedFile.FileName, baseSha);
                var right = await FetchFileContentFromRawUrl(SelectedFile.RawUrl);

                if (thisToken != _currentFileToken) return;

                LeftFileText = left;
                RightFileText = right;

                await BuildSideBySideDiffWithAISuggestions(left, right, SelectedFile.FileName, thisToken);
            }
        }

        private async Task BuildSideBySideDiffWithAISuggestions(string baseText, string prText, string fileName, Guid token)
        {
            var thisToken = token;
            var diffBuilder = new SideBySideDiffBuilder(new Differ());
            var diffModel = diffBuilder.BuildDiffModel(baseText ?? "", prText ?? "");

            DiffRows.Clear();

            var prFileLines = (prText ?? "").Replace("\r\n", "\n").Split('\n').ToList();
            int maxCount = Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count);
            for (int i = 0; i < maxCount; i++)
            {
                var left = i < diffModel.OldText.Lines.Count ? diffModel.OldText.Lines[i] : null;
                var right = i < diffModel.NewText.Lines.Count ? diffModel.NewText.Lines[i] : null;

                DiffLine leftLine = left != null && left.Type != DiffPlex.DiffBuilder.Model.ChangeType.Imaginary
                    ? CreateDiffLine(left, true)
                    : null;
                DiffLine rightLine = right != null && right.Type != DiffPlex.DiffBuilder.Model.ChangeType.Imaginary
                    ? CreateDiffLine(right, false)
                    : null;

                InlineAISuggestion suggestion = null;

                if (rightLine != null && (rightLine.Type == DiffType.Added || rightLine.Type == DiffType.Modified))
                {
                    // --- See if already applied ---
                    var key = (fileName, rightLine.LineNumber ?? i);
                    var applied = AppliedSuggestions.FirstOrDefault(s => s.FileName == fileName && s.LineNumber == (rightLine.LineNumber ?? i));
                    if (applied != null && applied.IsApplied && _appliedCodeMap.TryGetValue(key, out var appliedCode))
                    {
                        suggestion = applied;
                        rightLine.Text = appliedCode; // Show applied code
                    }
                    else
                    {
                        string aiResponse = await GetAISuggestionForLineAsync(
                            rightLine.Text,
                            rightLine.LineNumber ?? (i + 1),
                            fileName,
                            prFileLines
                        );
                        if (thisToken != _currentFileToken) return;

                        suggestion = new InlineAISuggestion
                        {
                            FileName = fileName,
                            LineNumber = rightLine.LineNumber ?? i,
                            OriginalText = rightLine.Text,
                            SuggestedText = aiResponse,
                            IsApplied = false
                        };
                    }
                }

                var row = new DiffRow
                {
                    Left = leftLine,
                    Right = rightLine,
                    InlineSuggestion = suggestion,
                    IsPopupOpen = false
                };

                DiffRows.Add(row);
            }

            if (thisToken != _currentFileToken) return;
            OnPropertyChanged(nameof(DiffRows));
        }

        private async Task FetchAISuggestionAsync(Guid token)
        {
            var thisToken = token;
            try
            {
                AISuggestion = "AI analyzing changes...";
                await Task.Delay(600); // Simulate

                if (thisToken != _currentFileToken) return;
                AISuggestion = "AI Suggestion: Consider better variable naming or refactor logic if possible.";
            }
            catch (Exception ex)
            {
                if (thisToken != _currentFileToken) return;
                AISuggestion = $"Failed to fetch AI suggestion: {ex.Message}";
            }
        }

        public async Task<string> GetAISuggestionForLineAsync(string codeLine, int lineNumber, string fileName, List<string> prFileLines)
        {
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["InlineAIReviewApiUrl"];

                var payload = new
                {
                    fileName = fileName,
                    line = codeLine,
                    lineNumber = lineNumber,
                    fullFile = prFileLines
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);
                var response = await HttpHelper.PostJsonAsync(apiUrl, jsonPayload);

                // Expected response: { "suggestedText": "..." }
                dynamic respObj = JsonConvert.DeserializeObject(response);
                return respObj?.suggestedText ?? "";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AI API error (line {lineNumber}): {ex.Message}");
                return ""; // If error, just skip suggestion
            }
        }

        private DiffType MapDiffType(DiffPlex.DiffBuilder.Model.ChangeType change)
        {
            return change switch
            {
                DiffPlex.DiffBuilder.Model.ChangeType.Unchanged => DiffType.Unchanged,
                DiffPlex.DiffBuilder.Model.ChangeType.Inserted => DiffType.Added,
                DiffPlex.DiffBuilder.Model.ChangeType.Deleted => DiffType.Removed,
                DiffPlex.DiffBuilder.Model.ChangeType.Imaginary => DiffType.Imaginary,
                _ => DiffType.Unchanged
            };
        }

        private DiffLine CreateDiffLine(DiffPiece piece, bool isLeftSide)
        {
            var line = new DiffLine
            {
                Text = piece.Text,
                Type = MapDiffType(piece.Type),
                LineNumber = piece.Type != DiffPlex.DiffBuilder.Model.ChangeType.Imaginary ? (int?)piece.Position : null,
                Words = new ObservableCollection<DiffWord>()
            };
            if (piece.SubPieces != null && piece.SubPieces.Count > 0)
            {
                foreach (var w in piece.SubPieces)
                    line.Words.Add(new DiffWord { Text = w.Text, Type = MapDiffType(w.Type) });
            }
            else
            {
                line.Words.Add(new DiffWord { Text = piece.Text, Type = MapDiffType(piece.Type) });
            }
            return line;
        }

        private async Task<string> FetchFileContentFromBase(string owner, string repo, string path, string sha)
        {
            try
            {
                string? token = Environment.GetEnvironmentVariable("GITHUB_PAT");
                string url = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}?ref={sha}";
                string json = await HttpHelper.GetAsync(url, token);
                dynamic obj = JsonConvert.DeserializeObject(json);

                string contentBase64 = obj.content;
                byte[] data = Convert.FromBase64String(contentBase64.Replace("\n", ""));
                return System.Text.Encoding.UTF8.GetString(data);
            }
            catch (Exception ex)
            {
                return $"Error loading base file: {ex.Message}";
            }
        }

        private async Task<string> FetchFileContentFromRawUrl(string rawUrl)
        {
            try
            {
                string? token = Environment.GetEnvironmentVariable("GITHUB_PAT");
                return await HttpHelper.GetAsync(rawUrl, token);
            }
            catch (Exception ex)
            {
                return $"Error loading PR file: {ex.Message}";
            }
        }

        // === Apply Suggestion (strip markdown, update both maps/lists, and UI) ===
        private void ApplyInlineSuggestion(InlineAISuggestion suggestion)
        {
            if (suggestion == null) return;

            var row = DiffRows.FirstOrDefault(r => r.InlineSuggestion == suggestion);
            if (row != null && row.Right != null)
            {
                string code = StripMarkdown(suggestion.SuggestedText);
                row.Right.Text = code;
                suggestion.IsApplied = true;
                row.IsPopupOpen = false;

                var key = (suggestion.FileName, suggestion.LineNumber);
                _appliedCodeMap[key] = code; // Persist stripped code for this file/line

                if (!AppliedSuggestions.Contains(suggestion))
                    AppliedSuggestions.Add(suggestion);
            }
            OnPropertyChanged(nameof(DiffRows));
        }

        // === Undo Suggestion (restore original text, remove from maps/lists, update UI) ===
        private void UndoInlineSuggestion(InlineAISuggestion suggestion)
        {
            if (suggestion == null) return;

            var row = DiffRows.FirstOrDefault(r => r.InlineSuggestion == suggestion);
            if (row != null && row.Right != null)
            {
                row.Right.Text = suggestion.OriginalText;
                suggestion.IsApplied = false;

                var key = (suggestion.FileName, suggestion.LineNumber);
                if (_appliedCodeMap.ContainsKey(key))
                    _appliedCodeMap.Remove(key);

                if (AppliedSuggestions.Contains(suggestion))
                    AppliedSuggestions.Remove(suggestion);
            }
            OnPropertyChanged(nameof(DiffRows));
        }

        // === Helper: Remove markdown ```
        private string StripMarkdown(string suggestion)
        {
            if (string.IsNullOrWhiteSpace(suggestion)) return "";
            string code = suggestion;
            if (code.StartsWith("```"))
            {
                int firstNewline = code.IndexOf('\n');
                if (firstNewline >= 0)
                    code = code.Substring(firstNewline + 1);
                int lastBackticks = code.LastIndexOf("```");
                if (lastBackticks > 0)
                    code = code.Substring(0, lastBackticks).TrimEnd();
            }
            return code.Trim();
        }

        private void OpenPopupForRow(DiffRow row)
        {
            foreach (var r in DiffRows)
                r.IsPopupOpen = false;

            if (row != null)
                row.IsPopupOpen = true;
            OnPropertyChanged(nameof(DiffRows));
        }

        private async void AnalyzeWithAI()
        {
            AiFeedback = "Analyzing...";
            try
            {
                var payloadObj = new Dictionary<string, object>
                {
                    { "repo_url", _repo.HtmlUrl },
                    { "pr_number", _pullRequest.Number },
                    { "base", _pullRequest.Base.Ref }
                };
                string jsonPayload = JsonConvert.SerializeObject(payloadObj);
                string apiUrl = ConfigurationManager.AppSettings["AIReviewApiUrl"];
                var response = await HttpHelper.PostJsonAsync(apiUrl, jsonPayload);

                AiReport = JsonConvert.DeserializeObject<AIReport>(response);
                AiFeedback = JsonConvert.SerializeObject(AiReport, Formatting.Indented);
                _navigate(new ReportViewModel(this.AiReport, _navigate, _github));
            }
            catch (Exception ex)
            {
                AiFeedback = $"Error: {ex.Message}";
            }
        }
    }
}
