using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XYCordReader.Models
{
    /// <summary>
    /// Základ všech tříd, kde se něco mění a je potřeba to odchytávat.
    /// </summary>
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private  void OnPropertyChanged(string[]? nameArray = null, string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            nameArray?.ForEach(OnPropertyChanged); //změny v závislých proměnných
        }

        protected bool SetValue(ref decimal target, decimal value, string[]? nameArray = null, [CallerMemberName] string? name = null)
        {
            if (target.Equals(value))
                return false;

            target = value;

            OnPropertyChanged(nameArray, name); //změny v bázové proměnné a v závislých proměnných

            return true;
        }

        protected bool SetValue(ref int target, int value, string[]? nameArray = null, [CallerMemberName] string? name = null)
        {
            if (target.Equals(value))
                return false;

            target = value;

            OnPropertyChanged(nameArray, name); //změny v bázové proměnné a v závislých proměnných

            return true;
        }

        protected bool SetValue(ref bool target, bool value, string[]? nameArray = null, [CallerMemberName] string? name = null)
        {
            if (target.Equals(value))
                return false;

            target = value;

            OnPropertyChanged(nameArray, name); //změny v bázové proměnné a v závislých proměnných

            return true;
        }

        protected bool SetValue(ref string target, string value, string[]? nameArray = null, [CallerMemberName] string? name = null)
        {
            if (target.Equals(value))
                return false;

            target = value;

            OnPropertyChanged(nameArray, name); //změny v bázové proměnné a v závislých proměnných

            return true;
        }

        #endregion

    }
}
