using System;
using System.Windows.Input;

namespace UI_01_BASIC_MVVM.Commands
{

    /// <summary>
    /// A relay command is a command where we can set the method on instanciation.
    /// This creates a more flexible command than creating a new command for each method.
    /// 
    /// source https://learn.microsoft.com/en-us/answers/questions/1188895/how-to-pass-a-parameter-using-a-relaycommand
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _action;
        public RelayCommand(Action<object> action)
        {
            _action = action;
            _canExecute = null;
        }

        public RelayCommand(Action<object> action, Predicate<object> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        
        
        }
        public void Execute(object o) => _action(o);
        public bool CanExecute(object o) => _canExecute == null || _canExecute(o);

        ///// <summary>
        ///// This method is used to update the UI when the command can execute changes.
        ///// </summary>
        //public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();


        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
