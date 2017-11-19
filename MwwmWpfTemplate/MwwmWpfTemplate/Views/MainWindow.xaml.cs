using System;
using System.Windows;
using MwwmWpfTemplate.ViewModels;
using System.Text;
using MwwmWpfTemplate.BLL;

namespace MwwmWpfTemplate.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                LogController.DefaultLoggerName = "MwwmWpfTemplate";

                MainWindowPanel.DataContext = MainWindowViewModel;
                MainWindowFrame.Title = _mainWindowViewModel.Title;
            }
            catch (Exception ex)
            {
                LogController.LogMessage(LogController.Level.Fatal, () => "Application failed while initializing MainWindow", ex);

                var exceptionString = new StringBuilder();
                var e = ex;
                while (null != e)
                {
                    exceptionString.AppendLine(e.Message);
                    e = e.InnerException;
                    exceptionString.AppendLine("+");
                }

                exceptionString.AppendLine("**********");
                exceptionString.AppendLine(ex.StackTrace);
                MessageBox.Show(exceptionString.ToString());
            }
        }

        #region Bindable properties

        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                if (null == _mainWindowViewModel)
                {
                    _mainWindowViewModel = new MainWindowViewModel();
                    _mainWindowViewModel.LogMessage += (sender, logArgs) => LogController.LogMessage(logArgs);
                    _mainWindowViewModel.Initialize();
                }
                return _mainWindowViewModel;
            }
        }

        #endregion

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _mainWindowViewModel?.PrepareShutdown();
            base.OnClosing(e);
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindowViewModel?.Dispose();
        }
    }
}

