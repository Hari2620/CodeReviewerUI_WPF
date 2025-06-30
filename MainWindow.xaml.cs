using System.ComponentModel;
using System.Windows;
using CodeReviewerApp.ViewModels;
using CodeReviewerApp.Service;
using CodeReviewerApp.Interface;
using ModernWpf;

namespace CodeReviewerApp
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();

            IGitHubService githubService = new GitHubService();
            _vm = new MainWindowViewModel(githubService);
            DataContext = _vm;
        }

        private void ToggleThemeButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme =
              ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;
        }
    }
}
