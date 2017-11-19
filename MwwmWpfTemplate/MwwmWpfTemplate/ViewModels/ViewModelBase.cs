using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MwwmWpfTemplate.Annotations;
using MwwmWpfTemplate.BLL;

namespace MwwmWpfTemplate.ViewModels
{
    public abstract class ViewModelBase : LogController.ILogEnabled, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// ViewModel initialization method, that optionally can be overriden in decendant ViewModels.        
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Override this method to do anything that needs to be done when closing down, eg. disconnect from hardware etc.
        /// </summary>
        public virtual void PrepareShutdown()
        {
        }

        #region Logging related members

        protected virtual void Log(Func<string> message, LogController.Level level = LogController.Level.Debug, Exception error = null)
        {
            Log(this, new LogController.LogMessageEventArgs { LoggerName = "", Level = level, Message = message, Error = error });
        }

        protected virtual void Log(LogController.LogMessageEventArgs e)
        {
            Log(this, e);
        }

        protected virtual void Log(object sender, LogController.LogMessageEventArgs e)
        {
            LogMessage?.Invoke(sender, e);
        }
        #endregion

        #region ILogEnabled implementaion

        public event EventHandler<LogController.LogMessageEventArgs> LogMessage;
        
        #endregion

        #region INotifyPropertyChanged implementation

        public static event Action<string> PropertyChangedEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            OnPropertyChanged(propertyName);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            // This methods implementation is stolen from Prism
            if (Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChangedEvent?.Invoke(propertyName);
        }

        #endregion

        #region IDisposable implementation (using Dispose pattern http://www.codeproject.com/KB/cs/idisposable.aspx)

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed; // to detect redundant calls

        /// <summary>
        ///     Override this method in inheriting classes to dipose any alloceted resources in those classes.
        ///     Remember to call base.Dispose(disposing) in inheriting classes Dispose(bool) methods.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose only here - that is, no finalizable logic
                }

                _disposed = true;
            }
        }

        ~ViewModelBase()
        {
            Dispose(false);
        }

        #endregion
    }
}