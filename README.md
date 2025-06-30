# CodeReviewerApp

A **modern WPF desktop tool** to automate AI-powered code reviews on GitHub Pull Requests.  
Fetch PRs, view full diffs, analyze changes via an AI microservice, and get actionable code quality feedback—right on your desktop.

---

## 🚀 Features

- **GitHub Integration**  
  - List repositories and branches for an authenticated user.
  - List and open Pull Requests for selected branches.
  - View unified PR diffs in a syntax-highlighted textbox.

- **AI Review Automation**
  - One-click AI analysis of code diffs via a FastAPI backend.
  - Get back rule-based and AI-driven suggestions (naming, complexity, security, comments).
  - Interactive, readable report UI with progress bars, sectioned suggestions, and violations.

- **Modern, Responsive UI**
  - Fully asynchronous WPF MVVM design.
  - Intuitive navigation with animated transitions and theming support.
  - Iconic buttons for all actions (MahApps.Metro, ModernWpf, Octicons, FontAwesome).

- **Easy to Extend**
  - Modular design, easy to plug in new AI models, review rules, or different code hosts.

---

## 🏗️ Technical Architecture

- **Frontend:**  
  - **WPF (.NET Core 8.0)**, C#, MVVM pattern
  - **UI Frameworks:**  
    - [ModernWpf](https://github.com/Kinnara/ModernWpf)
    - [MahApps.Metro.IconPacks](https://github.com/MahApps/MahApps.Metro.IconPacks)
    - [DiffPlex](https://github.com/mmanela/diffplex) (for diff parsing/highlighting)
    - [MdXaml](https://github.com/whistyun/MdXaml) (for markdown rendering, if enabled)

- **GitHub API:**  
  - [Octokit.NET](https://github.com/octokit/octokit.net)

- **AI Microservice:**  
  - **Backend:** Python FastAPI (run locally or remote)
  - Accepts PR data (`repo_url`, `pr_number`, `base`), returns a JSON code review report
  - Can use LLM (OpenAI GPT, local LLM, or your own AI) plus custom rule-checking (pylint, bandit, flake8 etc.)

- **Helpers:**  
  - Async HTTP (with proper error handling)
  - Value converters (WPF: `ZeroToVisibleConverter`)
  - AppSettings-driven config (API endpoints, tokens)

---

## 📦 NuGet Packages Used

- `Octokit` (GitHub REST API client)
- `Wpf.Ui` or `ModernWpf`
- `MahApps.Metro.IconPacks`
- `DiffPlex`
- `MdXaml`
- `Newtonsoft.Json`
- `System.Configuration.ConfigurationManager`

---

## 🖼️ Solution Structure

/CodeReviewerApp
├── Helpers/
│ ├─ HttpHelper.cs
│ └─ ZeroToVisibleConverter.cs
├── Interface/
│ └─ IGitHubService.cs
├── Models/
│ └─ AIReport.cs
├── Service/
│ └─ GitHubService.cs
├── View/
│ ├─ DiffView.xaml
│ ├─ PrListView.xaml
│ ├─ RepoBranchView.xaml
│ └─ ReportView.xaml
├── ViewModels/
│ ├─ DiffViewModel.cs
│ ├─ PrListViewModel.cs
│ ├─ RepoBranchViewModel.cs
│ ├─ ReportViewModel.cs
│ └─ MainWindowViewModel.cs
├── App.xaml, MainWindow.xaml
├── App.config (API endpoint, etc.)
└── ...

---

## ⚙️ Configuration

**1. GitHub Personal Access Token (PAT):**

- Set an environment variable `GITHUB_PAT` with your GitHub token.

**2. AI API Endpoint:**

- In `App.config`:
    ```xml
    <appSettings>
      <add key="AIReviewApiUrl" value="http://127.0.0.1:5000/review" />
    </appSettings>
    ```

---

## 🛠️ Running the App

1. **Start the AI FastAPI server**
    - Clone and run your Python FastAPI backend (see `/ai-server` or ask your AI dev).
    - Make sure it’s listening on the configured port.

2. **Launch the WPF App**
    - Build and run in Visual Studio (x64 recommended).
    - Log in to GitHub via your PAT.

3. **Workflow:**
    - Select a repo and branch
    - List open PRs
    - View a PR diff (unified view)
    - Click “Analyze with AI” (robot/bolt icon)
    - View the feedback in a modern, scrollable report
    - Optionally, download/print/share the report

---

## 🧠 How the AI Review Works

- App POSTs a JSON payload to the API like:
    ```json
    {
      "repo_url": "https://github.com/Hari2620/pr_checker_sandbox.git",
      "pr_number": 1,
      "base": "main"
    }
    ```
- Backend clones the repo (if needed), checks out PR, runs linters and LLM analysis.
- Returns:
    ```json
    {
      "naming_convention": [ ... ],
      "complexity": [ ... ],
      "security": [ ... ],
      "ai_comments": [ ... ],
      "ai_score": 0.85
    }
    ```
- App renders all this in a modern UI for review.

---

## 💡 Design/UX Notes

- Fully reactive UI: all GitHub/AI actions are async, UI never blocks.
- Modern look: card backgrounds, rounded borders, custom scrollbars, touch-friendly, dark/light themes.
- Everything is keyboard-accessible for speed (Tab, Enter, Escape).
- All user navigation happens through the `CurrentViewModel` property, which keeps the main window’s content flexible.

---

## 🧩 Extensibility

- Want to plug in a new AI backend? Just update the endpoint and response contract.
- Want to support Azure DevOps or GitLab? Add a new Service/Interface and wire up.
- Want to run more/different static analysis? Modify the FastAPI backend (pylint, bandit, flake8, custom scripts).

---

## 🛡️ Security

- Your PAT is only stored in env vars, never checked into code.
- API requests are local (by default)—change endpoint as needed.
- If integrating with a shared backend, add authentication (JWT, OAuth) to the FastAPI server.

---

## 📝 License

[MIT](LICENSE)

---

## 🙏 Credits

- Built by Hari.
- AI backend by Hariram.
- Modern UI via ModernWpf and MahApps.Metro.
- Special thanks to OpenAI, GitHub, and the open-source community.

---

## 📢 Issues? Suggestions?

Raise a GitHub issue or contact Hariram.

---

*Ready to save hours on code review and raise your team’s quality bar? Fire it up!*
