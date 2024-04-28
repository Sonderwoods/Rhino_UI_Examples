using System;
using System.Windows.Input;

namespace UI_01_BASIC_MVVM.Commands
{

    /// <summary>
    /// A relay command is a command where we can set the method on instanciation.
    /// This creates a more flexible command than creating a new command for each method.
    /// </summary>
    public class RelayCommand : ICommand
    {

        private readonly Action mAction;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; } // These values make sure that CanExecute is executed
            remove { CommandManager.RequerySuggested -= value; } 
        }

        public RelayCommand(Action action)
        {
            mAction = action;
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            mAction.Invoke();
        }


    }
}
