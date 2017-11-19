using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using MwwmWpfTemplate.BLL;
using MwwmWpfTemplate.Properties;
using MwwmWpfTemplate.Utils;
using MwwmWpfTemplate.Views;

namespace MwwmWpfTemplate.ViewModels
{
    /// <summary>
    /// ViewModel for the application main view.  This class holds all the displayed data that
    /// is databound to the MainWindow (the main screen so to speak).
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private static Mutex _mutex;

        private FrameworkElement _currentControl;
        private SplashWindow _splashWindow;
        private int _terminalNo;

        private MainViewUserControl _mainView;
        private MainViewViewModel _mainViewViewModel;
        private FeatureViewUserControl _featureView;
        private FeatureViewViewModel _featureViewViewModel;

        private ICommand _menuCloseCommand;
        private ICommand _menuAboutCommand;

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Initializes the viewmodel.  Should be called after LogginMessage eventhandler has been subscribed to.
        /// </summary>
        public override void Initialize()
        {
            if (IsAlreadyRunning())
            {
                MessageBox.Show(
                    "Another version af MwwmWpfTemplate.exe is already started.\r\n\r\nPlease close the program.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Application.Current.Shutdown(0);
            }
            else
            {
                try
                {
                    GetTerminalNo(false);

                    Log(() => "**********************************************************************************");
                    Log(() => $"{Title} starting and initializing");
                    Log(() => "**********************************************************************************");
                    Log(() => "Initializing MainModel (Connecting to database and loading configuration)...");

                    var splash = DisplaySplashScreen(false, "Initializing application...");

                    //DataBaseHelpers.ConfirmDataBaseVersion(5, 26);

                    splash.StatusText = "Loading configuration...";
                    //Configuration = _confManager.LoadConfiguration(TerminalNo);
                    //Thread.CurrentThread.CurrentUICulture = Configuration.UICulture;

                    splash.StatusText = "Connecting to hardware...";

                    // Setup views 
                    ChangeView(MainView);

                    Log(() => "Configuration has been read and MainWindowViewModel is initialized");
                }
                catch (Exception ex)
                {
                    Log(() => $"MainWindowViewModel Initialize failed: {ex.Message}", LogController.Level.Error, ex);
                    ShowSetupError(ex.Message);
                    Application.Current.Shutdown(-1);
                }
                finally
                {
                    RemoveSplashScreen();
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Called when mainform is shutting down. Do anything here that needs to be done when closing down, eg. disconnect from hardware etc.
        /// </summary>
        public override void PrepareShutdown()
        {
            //Log("MainWindowViewModel is preparing for shutdown");

            if (null != _mainViewViewModel)
            {
                _mainViewViewModel.PrepareShutdown();
                _mainViewViewModel.Dispose();
                _mainViewViewModel = null;
            }

            base.PrepareShutdown();
        }

        public void HandleShutdown()
        {
            if (MessageBox.Show("Close program?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                MessageBoxResult.Yes)
                return;

            PrepareShutdown();
            Application.Current.Shutdown();
        }

        #endregion

        #region Public and/or bindable properties

        #region Commands

        public ICommand MenuCloseCommand =>
            _menuCloseCommand ?? (_menuCloseCommand = new RelayCommand(arg => HandleShutdown(), null));

        public ICommand MenuAboutCommand =>
            _menuAboutCommand ?? (_menuAboutCommand = new RelayCommand(arg => HandleShowAbout(), null));

        #endregion

        public string Title =>
            $"{Resources.Title}, Version: {Assembly.GetExecutingAssembly().GetName().Version} (Terminal: {(TerminalNo == 0 ? "Unspecified" : TerminalNo.ToString())})";

        /// <summary> Holds currently displayed view in the MainWindow </summary>
        public FrameworkElement CurrentControl
        {
            get => _currentControl;
            set => SetProperty(ref _currentControl, value);
        }

        public FrameworkElement MainView => _mainView ?? (_mainView = new MainViewUserControl(MainViewViewModel));

        public MainViewViewModel MainViewViewModel
        {
            get {
                if (null == _mainViewViewModel)
                {
                    _mainViewViewModel = new MainViewViewModel();
                    _mainViewViewModel.LogMessage += Log;
                    _mainViewViewModel.DisplayFeatureViewAction = () => ChangeView(FeatureView);
                    _mainViewViewModel.Initialize();
                }
                return _mainViewViewModel;
            }
        }
        public FrameworkElement FeatureView => _featureView ?? (_featureView = new FeatureViewUserControl(FeatureViewViewModel));

        public FeatureViewViewModel FeatureViewViewModel
        {
            get {
                if (null == _featureViewViewModel)
                {
                    _featureViewViewModel = new FeatureViewViewModel();
                    _featureViewViewModel.LogMessage += Log;
                    _featureViewViewModel.ReturnAction = () => ChangeView(MainView);
                    _featureViewViewModel.Initialize();
                }
                return _featureViewViewModel;
            }
        }

        public int TerminalNo
        {
            get => _terminalNo;
            set => SetProperty(ref _terminalNo, value);
        }

        #endregion

        #region Private helper methods

        private void HandleShowAbout()
        {
            DisplaySplashScreen(true);
        }

        /// <summary> Displays a SplashScreen/Aboutbox with optional Close button </summary>
        /// <param name="showCloseButton">True: A Close button is displayed</param>
        /// <param name="statusText">Initial text displayed in StatusTextTextBlock</param>
        private SplashWindow DisplaySplashScreen(bool showCloseButton, string statusText = "")
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _splashWindow = new SplashWindow(Resources.Title, version, TerminalNo, statusText);

            if (false == showCloseButton)
            {
                // No close button => return view and let close it with RemoveSplashScreen() method
                _splashWindow.CloseButton.Visibility = Visibility.Hidden;
                _splashWindow.Show();
                return _splashWindow;
            }

            // Close button displayed => Show view as modal and use button to close it - and no need to return anything
            _splashWindow.StatusTextTextBlock.Visibility = Visibility.Hidden;
            _splashWindow.ShowDialog();
            return null;
        }

        private void RemoveSplashScreen()
        {
            if (null != _splashWindow)
            {
                _splashWindow.Close();
                if (_splashWindow is IDisposable) (_splashWindow as IDisposable).Dispose();
            }
        }

        private void ChangeView(FrameworkElement view)
        {
            if (null == view || Equals(CurrentControl, view)) return;
            CurrentControl = view;
        }

        private void ShowSetupError(string err)
        {
            MessageBox.Show(Resources.SetupError + "\r\n" + (err == null ? "" : "\r\nError: " + err + "\r\n\n") + Resources.ProgramStops,
                Resources.Title + " - " + Resources.SetupOrHardwareMissing, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void GetTerminalNo(bool throwIfNotFound = false)
        {
            // Determine TerminalNo from first commandline argument
            if (Environment.GetCommandLineArgs().Length < 2)
            {
                TerminalNo = 0;
                if (throwIfNotFound)
                {
                    Log(() => $"{Title} cannot initialize: CommandLine contains no TerminalNo argument. Program will shutdown.", LogController.Level.Error);
                    throw new ApplicationException(Resources.TerminalNoParseError + " ?" + Environment.NewLine + "Use ex. 'MwwmWpfTemplate.exe 21' in the program shortcut");
                }
            }
            else
            {
                var terminalNoArgument = Environment.GetCommandLineArgs()[1];
                if (false == int.TryParse(terminalNoArgument, out _terminalNo))
                    throw new ApplicationException(Resources.TerminalNoParseError + " " + terminalNoArgument);
            }
        }

        #endregion

        #region Displose implementation

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>Test whether an instance of this program is already running.</summary>
        /// <returns>TRUE if an instance is already running</returns>
        /// <see cref="http://www.daniweb.com/software-development/csharp/threads/104911/628169#post628169"/>
        private static bool IsAlreadyRunning()
        {
            var strLoc = Assembly.GetExecutingAssembly().Location;
            var fileInfo = new FileInfo(strLoc);
            var sExeName = fileInfo.Name;

            _mutex = new Mutex(true, "Global\\" + sExeName, out var bCreatedNew);
            if (bCreatedNew) _mutex.ReleaseMutex();

            return !bCreatedNew;
        }

        #endregion
    }
}