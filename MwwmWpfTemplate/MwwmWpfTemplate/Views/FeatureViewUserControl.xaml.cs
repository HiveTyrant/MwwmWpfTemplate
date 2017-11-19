using System.Windows.Controls;
using MwwmWpfTemplate.ViewModels;

namespace MwwmWpfTemplate.Views
{
    /// <summary>
    /// Interaction logic for FeatureViewUserControl.xaml
    /// </summary>
    public partial class FeatureViewUserControl : UserControl
    {
        public FeatureViewUserControl()
        {
            InitializeComponent();
        }

        public FeatureViewUserControl(FeatureViewViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
