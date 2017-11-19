using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using MwwmWpfTemplate.Utils;

namespace MwwmWpfTemplate.ViewModels
{
    public class FeatureViewViewModel : ViewModelBase
    {
        #region Private fields and properties

        private ICommand _returnCommand;

        #endregion

        #region Construction and initialization

        public override void Initialize()
        {
        }

        #endregion

        #region Public properties

        public Action ReturnAction { get; set; }

        #region Bindable properties used from MainViewUserControl
        public ICommand ReturnCommand => _returnCommand ?? (_returnCommand = new RelayCommand(arg => ReturnAction(), (o) => ReturnAction != null));

        #endregion

        #region ViewModels for sub-views
        #endregion

        #endregion

        #region Helper methods
        #endregion

        #region Override
        #endregion
    }
}