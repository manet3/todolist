using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ToDoList.Client.Views.Attached
{
    class ListViewAttached
    {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems", typeof(object[]), typeof(ListViewAttached),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedItemsChanged)));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
