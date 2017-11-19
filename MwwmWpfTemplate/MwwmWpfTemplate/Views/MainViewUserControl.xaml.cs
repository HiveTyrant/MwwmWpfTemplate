using System.Windows.Controls;
using MwwmWpfTemplate.ViewModels;

namespace MwwmWpfTemplate.Views
{
    /// <summary>
    /// Interaction logic for MainViewUserControl.xaml
    /// </summary>
    public partial class MainViewUserControl : UserControl
    {
        public MainViewUserControl()
        {
            InitializeComponent();
        }

        public MainViewUserControl(MainViewViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
