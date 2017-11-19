using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using MwwmWpfTemplate.Utils;

namespace MwwmWpfTemplate.ViewModels
{
    public class MainViewViewModel : ViewModelBase
    {
        #region Private fields and properties

        private ICommand _featureViewCommand;

        #endregion

        #region Construction and initialization

        public override void Initialize()
        {
        }

        #endregion

        #region Public properties

        public Action DisplayFeatureViewAction { get; set; }

        #region Bindable properties used from MainViewUserControl
        public ICommand FeatureViewCommand => _featureViewCommand ?? (_featureViewCommand = new RelayCommand(arg => DisplayFeatureViewAction(), (o) => DisplayFeatureViewAction != null));

        #endregion

        #region ViewModels for sub-views
        #endregion

        #endregion

        #region Helper methods
        #endregion

        #region Overrides
        #endregion
    }
}