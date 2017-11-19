using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace MwwmWpfTemplate.Views
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private string _statusText;

        public SplashWindow(string applicationTitle, string version, int terminalNo, string statusText)
        {
            ApplicationTitle = applicationTitle;
            Version = "Version: " + version;
            TerminalNo = "Terminal: " + terminalNo;
            _statusText = statusText;

            DataContext = this;
            InitializeComponent();
        }

        public string ApplicationTitle { get; set; }
        public string Version { get; set; }
        public string TerminalNo { get; set; }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (_statusText == value) return;
                _statusText = value;
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, (Action) (() =>
                {
                    StatusTextTextBlock.Text = value;
                }));

            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start((sender as Hyperlink)?.NavigateUri.ToString());
        }
    }
}
