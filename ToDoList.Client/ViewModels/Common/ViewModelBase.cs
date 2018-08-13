using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ToDoList.Client.ViewModels.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetValue<T>(ref T propertyVal, T newVal, [CallerMemberName]string property = null)
        {
            if (EqualityComparer<T>.Default.Equals(propertyVal, newVal))
                return;

            propertyVal = newVal;

            OnPropertyChanged(property);
        }

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
