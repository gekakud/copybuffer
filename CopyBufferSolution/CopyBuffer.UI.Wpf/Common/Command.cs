using System;
using System.Windows.Input;

namespace CopyBuffer.Ui.Wpf.Common
{
    public class Command : ICommand
    {
        #region Fields
        private Action _executeMethode;
        private Func<bool> _canExecuteMethode;
        #endregion

        #region Constructor
        public Command(Action p_executeMethode)
        {
            _executeMethode = p_executeMethode;
        }

        public Command(Action p_executeMethode, Func<bool> p_canExecuteMethode)
            : this(p_executeMethode)
        {
            _canExecuteMethode = p_canExecuteMethode;
        }

        #endregion

        #region Public
        public void RiseCanExcuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
        #endregion

        #region ICommand Members
        bool ICommand.CanExecute(object p_parameter)
        {
            if (_canExecuteMethode == null)
            {
                return true;
            }
            return _executeMethode != null && _canExecuteMethode();
        }

        void ICommand.Execute(object p_parameter)
        {
            if (_executeMethode == null)
            {
                return;
            }
            _executeMethode();
        }

        public event EventHandler CanExecuteChanged = delegate { };
        #endregion
    }

    public class Command<T> : ICommand
    {
        #region Fields
        private Action<T> _targetExecuteMethod;
        private Func<T, bool> _targetCanExcuteMethode;
        #endregion

        #region Constructor
        public Command(Action<T> p_execute)
        {
            _targetExecuteMethod = p_execute;
        }

        public Command(Action<T> p_execute, Func<T, bool> p_check)
            : this(p_execute)
        {
            _targetCanExcuteMethode = p_check;
        }
        #endregion

        #region ICommand members
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object p_parameter)
        {
            if (_targetCanExcuteMethode != null)
            {
                T tParam = (T)p_parameter;
                return _targetCanExcuteMethode(tParam);
            }
            return _targetExecuteMethod != null;
        }

        void ICommand.Execute(object p_parameter)
        {
            if (_targetExecuteMethod != null)
            {
                _targetExecuteMethod((T)p_parameter);
            }
        }

        public event EventHandler CanExecuteChanged = delegate { };
        #endregion
    }
}
