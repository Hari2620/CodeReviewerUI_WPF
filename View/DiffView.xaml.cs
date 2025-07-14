using System.Windows;
using System.Windows.Controls;
using CodeReviewerApp.Models;
using CodeReviewerApp.ViewModels;

namespace CodeReviewerApp.View
{
    /// <summary>
    /// Interaction logic for DiffView.xaml
    /// </summary>
    public partial class DiffView : UserControl
    {
        public DiffView()
        {
            InitializeComponent();
        }

        // Handles the click on the icon to toggle the AI suggestion popup
        private void AIIconBtn_Click(object sender, RoutedEventArgs e)
        {
            // Defensive: Make sure sender is a Button and find its DataContext
            var btn = sender as Button;
            if (btn == null)
                return;

            var diffRow = btn.DataContext as DiffRow;
            if (diffRow == null)
                return;

            // Optional: Close all other popups for a clean experience
            var vm = this.DataContext as DiffViewModel;
            if (vm != null)
            {
                foreach (var row in vm.DiffRows)
                {
                    if (row != diffRow)
                        row.ShowAISuggestionPopup = false;
                }
            }

            // Toggle this popup (click again to close, or always open if you prefer)
            diffRow.ShowAISuggestionPopup = !diffRow.ShowAISuggestionPopup;
        }
    }
}
