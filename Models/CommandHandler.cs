using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XYCordReader.Models
{
    /// <summary>
    /// Třída pro podporu volání metod Modelu z View.
    /// </summary>
    /// <example>
    /// View
    /// ----
    /// <Button Content="Uložit" Command="{Binding CloseApp}"/>
    /// 
    /// Model
    /// -----
    /// public ICommand CloseApp
    /// {
    ///     get => _CloseApp ??= new CommandHandler(() => CloseAppCommand(), true);
    ///     set => _CloseApp = value;
    /// }
    /// 
    /// private ICommand? _CloseApp;
    /// 
    /// private void CloseAppCommand()
    /// {
    ///     ...
    /// }
    /// </example>
    public class CommandHandler : ICommand
    {
        private Action _Action;
        private bool _CanExecute = true;

        public CommandHandler(Action action, bool canExecute)
        {
            _Action = action;
            _CanExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _CanExecute;

        public event EventHandler? CanExecuteChanged;

        public void Execute(object? parameter) => _Action();
    }
}
