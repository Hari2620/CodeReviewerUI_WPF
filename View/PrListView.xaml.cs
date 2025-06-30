using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodeReviewerApp.View
{
    /// <summary>
    /// Interaction logic for PrListView.xaml
    /// </summary>
    public partial class PrListView : UserControl
    {
        public PrListView()
        {
            InitializeComponent();
        }
        private void PrListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as ViewModels.PrListViewModel;
            if (vm?.OpenDiffCommand.CanExecute(null) == true)
                vm.OpenDiffCommand.Execute(null);
        }
    }
}
